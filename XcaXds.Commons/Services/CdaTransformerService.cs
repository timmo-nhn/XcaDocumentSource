using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.AccessControl;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public class CdaTransformerService
{
    private readonly ILogger<CdaTransformerService> _logger;
    public CdaTransformerService(ILogger<CdaTransformerService> logger)
    {
        _logger = logger;
    }
    public CdaTransformerService()
    {
            
    }

    /// <summary>
    /// Parse a provide and register request and transform into a CDA document.
    /// Will preserve the document content in the CDA documents NonXmlBody 
    /// </summary>
    public ClinicalDocument TransformProvideAndRegisterRequestToClinicalDocument(ProvideAndRegisterDocumentSetRequestType provideAndRegister)
    {
        var cdaDocument = new ClinicalDocument();

        var associations = provideAndRegister.SubmitObjectsRequest.RegistryObjectList.OfType<AssociationType>()
            .Where(assoc => assoc.AssociationTypeData == Constants.Xds.AssociationType.HasMember).ToArray();
        var extrinsicObjects = provideAndRegister.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = provideAndRegister.SubmitObjectsRequest.RegistryObjectList.OfType<RegistryPackageType>().ToArray();
        var documents = provideAndRegister.Document;

        foreach (var association in associations)
        {
            var documentForAssociation = documents?.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var extrinsicObjectForAssociation = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var registryPackageForAssociation = registryPackages.FirstOrDefault(rp => rp.Id.NoUrn() == association.SourceObject.NoUrn());

            // CDA.EffectiveTime
            var documentSubmissionTime = extrinsicObjectForAssociation.GetSlot(Constants.Xds.SlotNames.SubmissionTime).FirstOrDefault().GetFirstValue();

            if (DateTimeOffset.TryParseExact(
                    documentSubmissionTime?.Trim(),
                    Constants.Hl7.Dtm.DtmFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var submissionTime))
            {
                cdaDocument.EffectiveTime = new()
                {
                    EffectiveTime = submissionTime,
                    Value = documentSubmissionTime
                };
            }

            // CDA.confidentialityCode
            var extrinsicObjectConfCode = extrinsicObjectForAssociation.Classification
                .Where(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                .FirstOrDefault();

            if (extrinsicObjectConfCode is not null)
            {
                var cCodeOid = extrinsicObjectConfCode.Slot.FirstOrDefault().GetFirstValue();
                var cCodeDisplay = extrinsicObjectConfCode.Name.GetFirstValue();
                var ccodeValue = extrinsicObjectConfCode.NodeRepresentation;
            }

        }

        return cdaDocument;
    }

    /// <summary>
    /// Parse a CDA document and transform the content into a Registry object list IE for ITI-41.
    /// Will preserve the CDA document and its containing base64 document as part of the 
    /// </summary>
    public IdentifiableType[] TransformCdaDocumentToRegistryObjectList(ClinicalDocument cdaDocument)
    {
        var extrinsicObject = TransformCdaDocumentToExtrinsicObject(cdaDocument);
        var registryPackage = TransformCdaDocumentToRegistryPackage(cdaDocument);

        throw new NotImplementedException();
    }

    private ExtrinsicObjectType? TransformCdaDocumentToExtrinsicObject(ClinicalDocument cdaDocument)
    {
        if (cdaDocument == null) return null;

        var extrinsicObject = new ExtrinsicObjectType();

        // Mimetype
        if (cdaDocument.Component?.NonXmlBody?.Text?.MediaType != null)
        {
            extrinsicObject.MimeType = cdaDocument.Component.NonXmlBody.Text.MediaType;
        }

        // Slot submissionTime
        if (cdaDocument.EffectiveTime.Value != null)
        {
            extrinsicObject.AddSlot(new SlotType(
                Constants.Xds.SlotNames.SubmissionTime, 
                cdaDocument.EffectiveTime.Value 
                ?? DateTime.UtcNow.ToString(Constants.Hl7.Dtm.DtmFormat)));
        }

        // Slot sourcePatientId
        var patientId = cdaDocument.RecordTarget.FirstOrDefault()?.PatientRole.Id.FirstOrDefault();
        if (patientId != null)
        {
            var sourcePatientId = new CX()
            {
                IdNumber = patientId.Extension,
                AssigningAuthority = new HD()
                {
                    UniversalId = patientId.Root,
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso
                }
            };
            var sourcePatientIdString = sourcePatientId.Serialize();
            extrinsicObject.AddSlot(new SlotType(Constants.Xds.SlotNames.SourcePatientId, sourcePatientIdString));
        }

        // Slot sourcePatientInfo
        var patient = cdaDocument.RecordTarget.FirstOrDefault()?.PatientRole;
        var cdaPatientId = cdaDocument.RecordTarget.FirstOrDefault()?.PatientRole.Id.FirstOrDefault();

        if (patient != null && cdaPatientId != null)
        {
            // PID-3
            var patientIdentifier = new CX()
            {
                IdNumber = cdaPatientId.Extension,
                AssigningAuthority = new HD()
                {
                    UniversalId = cdaPatientId.Root,
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso
                }
            };
            var patientIdPidString = $"PID-3|{patientIdentifier.Serialize()}";


            // PID-5
            var patientName = new XPN()
            {
                FamilyName = patient.Patient?.Name?.FirstOrDefault()?.Family?.FirstOrDefault()?.Value ?? "",
                GivenName = patient.Patient?.Name?.FirstOrDefault()?.Given?.FirstOrDefault()?.Value ?? ""
            };
            var patientNameString = $"PID-5|{patientName.Serialize()}";

            // PID-7

            // PID-8
            var patientGender = patient.Patient?.AdministrativeGenderCode?.Code switch
            {
                "1" => "M",
                "2" => "F",
                "9" => "O",
                "0" => "U",
                 _  => "U"
            };
        }


        return extrinsicObject;
    }

    private RegistryPackageType? TransformCdaDocumentToRegistryPackage(ClinicalDocument cdaDocument)
    {
        if (cdaDocument == null) return null;

        var registryPackage = new RegistryPackageType();

        return registryPackage;
    }
}
