using Microsoft.Extensions.Logging;
using System.Globalization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Hl7.V2;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Source;

namespace XcaXds.WebService.Services;

public class Hl7RegistryService
{
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<XdsRegistryService> _logger;


    public Hl7RegistryService(ApplicationConfig appConfig, RegistryWrapper registryWrapper, ILogger<XdsRegistryService> logger)
    {
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _logger = logger;

    }

    public Message PatientDemographicsQueryGetPatientIdentifiersInRegistry(Message findCandidatesQuery)
    {
        var qpdSegment = findCandidatesQuery.Segments("QPD").FirstOrDefault();


        var patientIds = qpdSegment.GetAllFields().Where(f => f.Value.Contains("PID"));

        // QPD|IHE PDQ Query|Q1234|@PID.3.1^131169~@PID.5^Danser^Line

        var patientFields = patientIds.FirstOrDefault().Value.Split("~");

        var patient = new PID();
        patient.PatientIdentifier ??= new();


        for (int i = 0; i < patientFields.Length; i++)
        {
            var field = patientFields[i];
            if (field.StartsWith("@PID.3"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.PatientIdentifier = Hl7Object.Parse<CX>(value);
            }
            if (field.StartsWith("@PID.5"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.PatientName = Hl7Object.Parse<XPN>(value);
            }
            if (field.StartsWith("@PID.7"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.BirthDate = DateTime.ParseExact(value, Constants.Hl7.Dtm.AllFormats, CultureInfo.InvariantCulture);
            }
            if (field.StartsWith("@PID.8"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.Gender = value;
            }
        }

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();
        var extrinsicObjectPatientIds = documentRegistry
            .OfType<DocumentEntryDto>()
            .Select(eo => GetPatientIdentifiersFromDocumentEntryDto(eo))
            .ToList();


        var matchingPatientIds = extrinsicObjectPatientIds
            .Where(eop =>
            {
                bool nameMatch = eop.PatientName != null && 
                                 patient.PatientName != null &&
                                 !string.IsNullOrEmpty(patient.PatientName?.GivenName) &&
                                 eop.PatientName.GivenName?.Contains(patient.PatientName?.GivenName) == true ||
                                 !string.IsNullOrEmpty(patient.PatientName?.FamilyName) &&
                                 eop.PatientName?.FamilyName?.Contains(patient.PatientName?.FamilyName) == true;

                bool birthDateMatch = eop.BirthDate != DateTime.MinValue && 
                                      patient.BirthDate != DateTime.MinValue &&
                                      eop.BirthDate == patient.BirthDate;

                bool genderMatch = !string.IsNullOrEmpty(eop.Gender) &&
                                   eop.Gender == patient.Gender;

                bool identifierMatch = eop.PatientIdentifier != null &&
                                       patient.PatientIdentifier != null &&
                                       !string.IsNullOrEmpty(patient.PatientIdentifier.IdNumber) &&
                                       eop.PatientIdentifier.IdNumber?.Contains(patient.PatientIdentifier.IdNumber) == true;

                // Secret wildcard to get all patient identifiers!
                if (patient.PatientIdentifier?.IdNumber == "*")
                {
                    identifierMatch = true;
                }

                return nameMatch || birthDateMatch || genderMatch || identifierMatch;
            })
            .ToList();

        var enc = new HL7Encoding();

        var responseMessage = new Message();
        responseMessage.Encoding = enc;

        var reqMsh = findCandidatesQuery.Segments("MSH")[0].GetAllFields();

        responseMessage.AddSegmentMSH
        (
            sendingApplication: reqMsh[4].Value, // Sending app becomes receiving app
            sendingFacility: reqMsh[5].Value, // Sending facility becomes receiving facility
            receivingApplication: reqMsh[2].Value, // Receiving app becomes sending app
            receivingFacility: reqMsh[3].Value, // Receiving facility becomes sending facility
            security: DateTime.Now.ToString(Constants.Hl7.Dtm.DtmYmdFormat),
            messageType: "RSP^K22^RSP_K21",
            messageControlID: reqMsh[9].Value, // Message control ÌD
            processingID: "T", // Processing ID,
            version: reqMsh[11].Value // HL7 version
        );

        foreach (var pid in matchingPatientIds)
        {
            responseMessage.AddNewSegment(new Segment(enc)
            {
                Name = "PID",
                Value = "PID" + pid.Serialize('|')
            });
        }
        return responseMessage;
    }

    private PID GetPatientIdentifiersFromDocumentEntryDto(DocumentEntryDto eo)
    {
        throw new NotImplementedException();
    }

    public PID GetPatientIdentifiersFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var patientPid = new PID();
        patientPid.PatientIdentifier ??= new CX();

        var patientId = extrinsicObject.ExternalIdentifier.FirstOrDefault(x => x.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.PatientId)?.Value;

        var sourcePatientInfo = extrinsicObject.Slot?
        .FirstOrDefault(s => s.Name == Constants.Xds.SlotNames.SourcePatientInfo)?.ValueList?.Value?
        .ToList() ?? new List<string>();

        patientPid.PatientIdentifier = Hl7Object.Parse<CX>(patientId);

        foreach (var pidPart in sourcePatientInfo)
        {
            if (pidPart.Contains("PID-5"))
            {
                var value = pidPart.Substring(pidPart.IndexOf("|") + 1);
                patientPid.PatientName = Hl7Object.Parse<XPN>(value);
            }
            if (pidPart.Contains("PID-7"))
            {
                var value = pidPart.Substring(pidPart.IndexOf("|") + 1);
                patientPid.BirthDate = DateTime.ParseExact(value, Constants.Hl7.Dtm.AllFormats, CultureInfo.InvariantCulture);
            }
            if (pidPart.Contains("PID-8"))
            {
                var value = pidPart.Substring(pidPart.IndexOf("|") + 1);
                patientPid.Gender = value;
            }
        }
        return patientPid;

    }
}