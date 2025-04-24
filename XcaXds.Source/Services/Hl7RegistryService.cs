using Efferent.HL7.V2;
using XcaXds.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Source.Services;

public partial class RegistryService
{
    public Message PatientDemographicsQueryGetPatientIdentifiersInRegistry(Message findCandidatesQuery)
    {
        var qpdSegment = findCandidatesQuery.Segments("QPD").FirstOrDefault();
        var extrinsicObjectPatientIds = _documentRegistry.RegistryObjectList
            .OfType<ExtrinsicObjectType>()
            .Select(eo => new PatientInfoDto
            {
                PatientId = eo.ExternalIdentifier.FirstOrDefault(x => x.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.PatientId)?.Value,

                SourcePatientInfo = eo.Slot
                .FirstOrDefault(s => s.Name == "sourcePatientInfo")?
                .ValueList?.Value
                ?.ToList() ?? new List<string>()
            })
            .ToList();

        var patientIds = qpdSegment.GetAllFields().Where(f => f.Value.Contains("PID"));

        // QPD|IHE PDQ Query|Q1234|@PID.3.1^131169~@PID.5^Danser^Line

        var patientFields = patientIds.FirstOrDefault().Value.Split("~");
        var patient = new PID();
        foreach (var field in patientFields)
        {
            if (field.Contains("PID.3"))
            {
                var value = field.Split("^").Last().Trim();
                patient.PatientIdentifiers.Add(value);
            }
            if (field.Contains("PID.5"))
            {
                var value = field.Split("^").Last().Trim();
                patient.PatientIdentifiers.Add(value);
            }
        }

        //var filteredObjects = extrinsicObjectPatientIds.Where(pi => qpdSegment.GetAllFields().Select(pi.PatientId);
        throw new NotImplementedException();
    }
}
