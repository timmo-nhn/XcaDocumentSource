using System.Globalization;
using Microsoft.Extensions.Logging;
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
            var document = documents?.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var extrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var registryPackage = registryPackages.FirstOrDefault(rp => rp.Id.NoUrn() == association.SourceObject.NoUrn());

            if (extrinsicObject == null) continue;


            // ClinicalDocument.Id (ExtrinsicObject.RepositoryUniqueId & ExtrinsicObject.Classification.XDSDocumentEntry.uniqueId)
            SetClinicalDocumentId(cdaDocument, extrinsicObject);

            // ClinicalDocument.Code (ExtrinsicObject.TypeCode)
            SetClinicalDocumentTypeCode(cdaDocument, extrinsicObject);

            // ClinicalDocument.Title (ExtrinsicObject.Name)
            SetClinicalDocumentTitle(cdaDocument, extrinsicObject);

            // ClinicalDocument.EffectiveTime (ExtrinsicObject.Slot_CreationTime)
            SetClinicalDocumentEffectiveTime(cdaDocument, extrinsicObject);

            // ClinicalDocument.ConfidentialityCode (ExtrinsicObject.Classification_ConfidentialityCode)
            SetClinicalDocumentConfidentialityCode(cdaDocument, extrinsicObject);

            // ===============================
            // ClinicalDocument.recordTarget
            // ===============================
            SetClinicalDocumentRecordTarget(cdaDocument, extrinsicObject);

        }

        return cdaDocument;

    }

    private static void SetClinicalDocumentRecordTarget(ClinicalDocument cdaDocument, ExtrinsicObjectType? extrinsicObject)
    {
        var recordTarget = new RecordTarget();
        var patientRole = new PatientRole();

        // PatientRole.id (ExtrinsicObject.Slot_sourcePatientId)
        SetPatientRoleId(extrinsicObject, patientRole);

        recordTarget.PatientRole = patientRole;

        // ===============================
        // recordTarget.PatientRole.Patient
        // ===============================
        var patient = new Patient();

        var patientInfo = extrinsicObject.GetSlot(Constants.Xds.SlotNames.SourcePatientInfo)
            .FirstOrDefault()?
            .GetValues() ?? Array.Empty<string>();


        // Patient.Name (ExtrinsicObject.Slot_sourcePatientInfo)
        var patientName = patientInfo
            .FirstOrDefault(val => val.Contains("PID-5"))?.Split("PID-5|").FirstOrDefault();

        SetPatientName(patient, patientName);

        // Patient.administrativeGenderCode (ExtrinsicObject.Slot_sourcePatientInfo)
        var patientGender = patientInfo
            .FirstOrDefault(val => val.Contains("PID-8"))?.Split("PID-8|").FirstOrDefault();

        SetPatientAdministrativeGenderCode(patient, patientGender);

        // Patient.birthTime (ExtrinsicObject.Slot_sourcePatientInfo)
        var patientBirthTime = patientInfo
            .FirstOrDefault(val => val.Contains("PID-7"))?.Split("PID-7|").FirstOrDefault();

        SetPatientBirthTime(patient, patientBirthTime);

        recordTarget.PatientRole.Patient = patient;

        // ===============================
        // recordTarget.PatientRole.ProviderOrganization
        // ===============================
        var providerOrganization = new Organization();

        // ProviderOrganization.id (ExtrinsicObject.Classification_Author)
        var authorClassification = extrinsicObject.Classification.FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.Author);
        var authorSlots = authorClassification.GetSlot(Constants.Xds.SlotNames.AuthorInstitution).GetValues();

        var authorSlotCx = authorSlots.Select(asl => Hl7Object.Parse<CX>(asl)).ToArray();

        var department = authorSlotCx
            .FirstOrDefault(
            ascx => ascx.AssigningFacility != null && 
            (!ascx.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg) || 
            !ascx.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotCx.FirstOrDefault();

        var organization = authorSlotCx
            .FirstOrDefault(
            ascx => ascx.AssigningFacility != null &&
            (ascx.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg) || 
            ascx.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotCx.LastOrDefault();

        providerOrganization.Id ??= new();
        providerOrganization.Id.Add(new() 
        {
            Extension = department.IdNumber, 
            Root = department.AssigningFacility?.UniversalId ?? department.AssigningAuthority?.UniversalId 
        });

        providerOrganization.Name ??= new();


        cdaDocument.RecordTarget ??= new();
        cdaDocument.RecordTarget.Add(recordTarget);
    }

    private static void SetPatientBirthTime(Patient patient, string? patientBirthTime)
    {
        if (!string.IsNullOrWhiteSpace(patientBirthTime) && patient != null)
        {
            patient.BirthTime = new()
            {
                EffectiveTime = DateTime.ParseExact(patientBirthTime, Constants.Hl7.Dtm.DtmYmdFormat, CultureInfo.InvariantCulture),
            };
        }
    }

    private static void SetPatientAdministrativeGenderCode(Patient patient, string? patientGender)
    {
        if (patient != null)
        {
            patient.AdministrativeGenderCode ??= new();
            patient.AdministrativeGenderCode.Code = patientGender switch
            {
                "U" => "0",
                "M" => "1",
                "F" => "2",
                "O" => "9",
                _ => "0"
            };
            patient.AdministrativeGenderCode.CodeSystem = Constants.Oid.CodeSystems.Volven.Gender;
        }
    }

    private static void SetPatientName(Patient patient, string? patientName)
    {
        if (!string.IsNullOrWhiteSpace(patientName))
        {
            var name = Hl7Object.Parse<XPN>(patientName);
            patient.Name ??= new();
            patient.Name.Add(new()
            {
                Given = [new() { Value = name.GivenName }],
                Family = [new() { Value = name.FamilyName }]
            });
        }
    }

    private static void SetPatientRoleId(ExtrinsicObjectType extrinsicObject, PatientRole patientRole)
    {
        var sourcePatientIdSlot = extrinsicObject.GetSlot(Constants.Xds.SlotNames.SourcePatientId).FirstOrDefault()?.GetFirstValue();
        if (sourcePatientIdSlot != null)
        {
            var patientId = Hl7Object.Parse<CX>(sourcePatientIdSlot);
            patientRole.Id ??= new();
            patientRole.Id.Add(new()
            {
                Root = patientId.AssigningAuthority.UniversalId,
                Extension = patientId.IdNumber
            });
        }
    }

    private static void SetClinicalDocumentTitle(ClinicalDocument cdaDocument, ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectName = extrinsicObject.Name.GetFirstValue();
        cdaDocument.Title = extrinsicObjectName;
    }

    private static void SetClinicalDocumentConfidentialityCode(ClinicalDocument cdaDocument, ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectConfCode = extrinsicObject.Classification
            .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode);

        if (extrinsicObjectConfCode is not null)
        {
            var cCodeOid = extrinsicObjectConfCode.Slot.FirstOrDefault().GetFirstValue();
            var cCodeDisplay = extrinsicObjectConfCode.Name.GetFirstValue();
            var ccodeValue = extrinsicObjectConfCode.NodeRepresentation;

            cdaDocument.ConfidentialityCode = new()
            {
                Code = ccodeValue,
                DisplayName = cCodeDisplay,
                CodeSystem = cCodeOid
            };
        }
    }

    private static void SetClinicalDocumentEffectiveTime(ClinicalDocument cdaDocument, ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectSubmissionTime = extrinsicObject.GetSlot(Constants.Xds.SlotNames.SubmissionTime).FirstOrDefault()?.GetFirstValue() ??
                                            extrinsicObject.GetSlot(Constants.Xds.SlotNames.CreationTime).FirstOrDefault()?.GetFirstValue();

        if (DateTimeOffset.TryParseExact(
                extrinsicObjectSubmissionTime?.Trim(),
                Constants.Hl7.Dtm.DtmFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var submissionTime))
        {
            cdaDocument.EffectiveTime = new()
            {
                EffectiveTime = submissionTime,
                Value = extrinsicObjectSubmissionTime
            };
        }
    }

    private static void SetClinicalDocumentTypeCode(ClinicalDocument cdaDocument, ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectTypeCode = extrinsicObject.Classification
            .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.TypeCode);

        if (extrinsicObjectTypeCode is not null)
        {
            var cCodeOid = extrinsicObjectTypeCode.Slot.FirstOrDefault().GetFirstValue();
            var cCodeDisplay = extrinsicObjectTypeCode.Name.GetFirstValue();
            var ccodeValue = extrinsicObjectTypeCode.NodeRepresentation;

            cdaDocument.Code = new()
            {
                Code = ccodeValue,
                DisplayName = cCodeDisplay,
                CodeSystem = cCodeOid
            };
        }
    }

    static void SetClinicalDocumentId(ClinicalDocument cdaDocument, ExtrinsicObjectType extrinsicObject)
    {
        var repositoryId = extrinsicObject.GetSlot(Constants.Xds.SlotNames.RepositoryUniqueId).FirstOrDefault()?.GetFirstValue();
        var documentUniqueId = extrinsicObject.ExternalIdentifier.FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.UniqueId);
        cdaDocument.Id = new()
        {
            Root = repositoryId ?? string.Empty,
            Extension = documentUniqueId.Value ?? Guid.NewGuid().ToString(),
        };
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
                _ => "U"
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
