using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;
using CE = XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types.CE;
using TS = XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types.TS;

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
    public ClinicalDocument TransformProvideAndRegisterRequestToClinicalDocument(IdentifiableType[] registryObjects, DocumentType[] documents)
    {
        var cdaDocument = new ClinicalDocument();

        var associations = registryObjects.OfType<AssociationType>()
            .Where(assoc => assoc.AssociationTypeData == Constants.Xds.AssociationType.HasMember).ToArray();
        var extrinsicObjects = registryObjects.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = registryObjects.OfType<RegistryPackageType>().ToArray();

        foreach (var association in associations)
        {
            var document = documents?.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var extrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var registryPackage = registryPackages.FirstOrDefault(rp => rp.Id.NoUrn() == association.SourceObject.NoUrn());

            if (extrinsicObject == null) continue;


            // ClinicalDocument.Id (ExtrinsicObject.RepositoryUniqueId & ExtrinsicObject.Classification.XDSDocumentEntry.uniqueId)
            cdaDocument.Id = SetClinicalDocumentId(extrinsicObject);

            // ClinicalDocument.Code (ExtrinsicObject.TypeCode)
            cdaDocument.Code = SetClinicalDocumentTypeCode(extrinsicObject);

            // ClinicalDocument.Title (ExtrinsicObject.Name)
            cdaDocument.Title = SetClinicalDocumentTitle(extrinsicObject);

            // ClinicalDocument.EffectiveTime (ExtrinsicObject.Slot_CreationTime)
            cdaDocument.EffectiveTime = SetClinicalDocumentEffectiveTime(extrinsicObject);

            // ClinicalDocument.ConfidentialityCode (ExtrinsicObject.Classification_ConfidentialityCode)
            cdaDocument.ConfidentialityCode = SetClinicalDocumentConfidentialityCode(extrinsicObject);

            // ===============================
            // ClinicalDocument.recordTarget
            // ===============================
            cdaDocument.RecordTarget ??= new();
            cdaDocument.RecordTarget.Add(SetClinicalDocumentRecordTarget(extrinsicObject));

            // ===============================
            // ClinicalDocument.author
            // ===============================
            cdaDocument.Author ??= new();
            cdaDocument.Author.Add(SetClinicalDocumentAuthor(registryPackage));

            // ===============================
            // ClinicalDocument.custodian
            // ===============================
            cdaDocument.Custodian ??= new();
            cdaDocument.Custodian = SetClinicalDocumentCustodian(registryPackage);

            // ===============================
            // ClinicalDocument.nonXmlBody
            // ===============================
            if (document != null)
            {
                cdaDocument.Component ??= new();
                cdaDocument.Component.NonXmlBody = SetClinicalDocumentNonXmlBody(document, extrinsicObject);
            }
        }

        return cdaDocument;
    }

    private NonXmlBody? SetClinicalDocumentNonXmlBody(DocumentType document, ExtrinsicObjectType extrinsicObject)
    {
        var nonXmlBody = new NonXmlBody();
        nonXmlBody.Text ??= new();
        nonXmlBody.Text.MediaType = extrinsicObject.MimeType;
        nonXmlBody.Text.Text = Encoding.UTF8.GetString(document.Value);

        return nonXmlBody;
    }

    private Author SetClinicalDocumentAuthor(RegistryPackageType registryPackage)
    {
        var author = new Author();



        author.Time = SetAuthorTime(registryPackage);

        author.AssignedAuthor = SetAssignedAuthor(registryPackage);

        return author;
    }

    private AssignedAuthor SetAssignedAuthor(RegistryPackageType registryPackage)
    {
        var assignedAuthor = new AssignedAuthor();

        var authorPersonSlot = registryPackage.Classification
            .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.SubmissionSet.Author)?.GetSlots(Constants.Xds.SlotNames.AuthorPerson).GetValues();

        var authorPersonXcn = authorPersonSlot?.Select(asl => Hl7Object.Parse<XCN>(asl)).FirstOrDefault();

        if (authorPersonXcn != null)
        {
            assignedAuthor.Id ??= new();
            assignedAuthor.Id.Add(new()
            {
                Extension = authorPersonXcn?.PersonIdentifier ?? string.Empty,
                Root = authorPersonXcn?.AssigningAuthority?.UniversalId ?? string.Empty
            });

            assignedAuthor.AssignedPerson = SetAssignedPerson(authorPersonXcn);
        }
        return assignedAuthor;
    }

    private Person? SetAssignedPerson(XCN authorName)
    {
        var assignedPerson = new Person();

        assignedPerson.Name ??= new();

        assignedPerson.Name.Add(new()
        {
            Given = [new() { Value = authorName.GivenName }],
            Family = [new() { Value = authorName.FamilyName }]
        });

        return assignedPerson;
    }

    private TS SetAuthorTime(RegistryPackageType registryPackage)
    {
        var submissionTimeSlotValue = registryPackage.GetSlots(Constants.Xds.SlotNames.SubmissionTime).FirstOrDefault()?.GetFirstValue() ?? string.Empty;

        DateTimeOffset submissionTimeValue = new();

        if (DateTimeOffset.TryParseExact(
        submissionTimeSlotValue?.Trim(),
        Constants.Hl7.Dtm.DtmFormat,
        CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
        out var submissionTime))
        {
            submissionTimeValue = submissionTime;
        }

        return new()
        {
            EffectiveTime = submissionTimeValue,
            Value = submissionTimeSlotValue
        };
    }

    private Custodian SetClinicalDocumentCustodian(RegistryPackageType registryPackage)
    {
        var custodian = new Custodian();

        var authorOrganization = registryPackage.Classification
            .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.SubmissionSet.Author)?
            .GetSlots(Constants.Xds.SlotNames.AuthorInstitution)
            .FirstOrDefault()?
            .GetValues();

        if (authorOrganization != null)
        {
            var authorOrganizationXon = authorOrganization
                .Select(aos => Hl7Object.Parse<XON>(aos))
                .OrderByDescending(xon =>
                    xon.AssigningAuthority != null &&
                    !string.IsNullOrWhiteSpace(xon.AssigningAuthority.UniversalId))
                .ToArray();

            custodian.AssignedCustodian ??= new();
            custodian.AssignedCustodian.RepresentedCustodianOrganization = SetRepresentedCustodianOrganization(authorOrganizationXon.FirstOrDefault());
        }
        return custodian;
    }

    private CustodianOrganization SetRepresentedCustodianOrganization(XON? authorOrganization)
    {
        var custodianOrganization = new CustodianOrganization();

        if (authorOrganization != null)
        {
            custodianOrganization.Id ??= new();
            custodianOrganization.Id.Add(new()
            {
                Extension = authorOrganization?.OrganizationIdentifier,
                Root = authorOrganization?.AssigningAuthority?.UniversalId ?? authorOrganization?.AssigningFacility?.UniversalId
            });

            custodianOrganization.Name ??= new();
            custodianOrganization.Name.XmlText = authorOrganization.OrganizationName;
        }

        return custodianOrganization;
    }

    private static RecordTarget SetClinicalDocumentRecordTarget(ExtrinsicObjectType? extrinsicObject)
    {
        var recordTarget = new RecordTarget();
        var patientRole = new PatientRole();

        // PatientRole.id (ExtrinsicObject.Slot_sourcePatientId)
        recordTarget.PatientRole = SetPatientRole(extrinsicObject);


        return recordTarget;
    }

    private static TS SetPatientBirthTime(string? patientBirthTime)
    {
        if (!string.IsNullOrWhiteSpace(patientBirthTime))
        {
            return new()
            {
                EffectiveTime = DateTime.ParseExact(patientBirthTime, Constants.Hl7.Dtm.DtmYmdFormat, CultureInfo.InvariantCulture),
            };
        }
        return null;
    }

    private static CE SetPatientAdministrativeGenderCode(string? patientGender)
    {
        if (patientGender != null)
        {
            return new()
            {
                Code = patientGender switch
                {
                    "U" => "0",
                    "M" => "1",
                    "F" => "2",
                    "O" => "9",
                    _ => "0"
                },
                CodeSystem = Constants.Oid.CodeSystems.Volven.Gender,
                DisplayName = patientGender switch
                {
                    "U" => "Not known",
                    "M" => "Male",
                    "F" => "Female",
                    "O" => "Not applicable",
                    _ => "Not known"
                }
            };
        }
        return null;
    }

    private static PN SetPatientName(string? patientName)
    {
        if (!string.IsNullOrWhiteSpace(patientName))
        {
            var name = Hl7Object.Parse<XPN>(patientName);

            return new()
            {
                Given = [new() { Value = name.GivenName }],
                Family = [new() { Value = name.FamilyName }]
            };
        }
        return null;
    }

    private static PatientRole SetPatientRole(ExtrinsicObjectType extrinsicObject)
    {
        var patientRole = new PatientRole();
        // PatientRole.Id
        patientRole.Id ??= new();
        patientRole.Id.Add(SetPatientRoleId(extrinsicObject));

        // recordTarget.PatientRole.Patient
        var patient = new Patient();

        var patientInfo = extrinsicObject.GetSlots(Constants.Xds.SlotNames.SourcePatientInfo)
            .FirstOrDefault()?
            .GetValues() ?? Array.Empty<string>();


        // Patient.Name (ExtrinsicObject.Slot_sourcePatientInfo)
        var patientName = patientInfo
            .FirstOrDefault(val => val.Contains("PID-5"))?.Split("PID-5|").LastOrDefault();
        patient.Name ??= new();
        patient.Name.Add(SetPatientName(patientName));

        // Patient.administrativeGenderCode (ExtrinsicObject.Slot_sourcePatientInfo)
        var patientGender = patientInfo
            .FirstOrDefault(val => val.Contains("PID-8"))?.Split("PID-8|").LastOrDefault();
        patient.AdministrativeGenderCode = SetPatientAdministrativeGenderCode(patientGender);

        // Patient.birthTime (ExtrinsicObject.Slot_sourcePatientInfo)
        var patientBirthTime = patientInfo
            .FirstOrDefault(val => val.Contains("PID-7"))?.Split("PID-7|").LastOrDefault();
        patient.BirthTime = SetPatientBirthTime(patientBirthTime);

        patientRole.Patient = patient;

        // ===============================
        // recordTarget.PatientRole.ProviderOrganization
        // ===============================
        var providerOrganization = new Organization();

        // ProviderOrganization.id (ExtrinsicObject.Classification_Author)
        var authorClassification = extrinsicObject.Classification.FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.Author);
        var authorSlots = authorClassification.GetSlots(Constants.Xds.SlotNames.AuthorInstitution).GetValues();

        var authorSlotCx = authorSlots.Select(asl => Hl7Object.Parse<XON>(asl)).ToArray();

        var department = authorSlotCx
            .FirstOrDefault(ascx =>
                (ascx?.AssigningFacility?.UniversalId != null && !ascx.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg)) ||
                (ascx?.AssigningAuthority?.UniversalId != null && !ascx.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotCx.FirstOrDefault();

        var organization = authorSlotCx
            .FirstOrDefault(ascx =>
                (ascx?.AssigningFacility?.UniversalId != null && ascx.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg)) ||
                (ascx?.AssigningAuthority?.UniversalId != null && ascx.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotCx.LastOrDefault();

        if (department != null)
        {
            providerOrganization.Id ??= new();
            providerOrganization.Id.Add(new()
            {
                Extension = department.OrganizationIdentifier,
                Root = department.AssigningFacility?.UniversalId ?? department.AssigningAuthority?.UniversalId
            });


            providerOrganization.Name ??= new();
            providerOrganization.Name.Add(new()
            {
                XmlText = organization.OrganizationName
            });
        }

        if (organization != null)
        {
            var organizationPartOf = new OrganizationPartOf();
            organizationPartOf.WholeOrganization ??= new();
            organizationPartOf.WholeOrganization.Id ??= new();

            organizationPartOf.WholeOrganization.Id.Add(new()
            {
                Extension = organization.OrganizationIdentifier,
                Root = organization.AssigningFacility?.UniversalId ?? organization.AssigningAuthority?.UniversalId
            });

            organizationPartOf.WholeOrganization.Name ??= new();
            organizationPartOf.WholeOrganization.Name.Add(new()
            {
                XmlText = organization.OrganizationName
            });

            providerOrganization.AsOrganizationPartOf = organizationPartOf;
        }
        patientRole.ProviderOrganization = providerOrganization;

        return patientRole;
    }

    private static II SetPatientRoleId(ExtrinsicObjectType extrinsicObject)
    {
        var sourcePatientIdSlot = extrinsicObject.GetSlots(Constants.Xds.SlotNames.SourcePatientId).FirstOrDefault()?.GetFirstValue();
        if (sourcePatientIdSlot != null)
        {
            var patientId = Hl7Object.Parse<CX>(sourcePatientIdSlot);
            return new()
            {
                Root = patientId.AssigningAuthority.UniversalId,
                Extension = patientId.IdNumber
            };
        }
        return null;
    }

    private static string SetClinicalDocumentTitle(ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectName = extrinsicObject.Name.GetFirstValue();
        return extrinsicObjectName;
    }

    private static CV SetClinicalDocumentConfidentialityCode(ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectConfCode = extrinsicObject.Classification
            .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode);

        if (extrinsicObjectConfCode is not null)
        {
            var cCodeOid = extrinsicObjectConfCode.Slot.FirstOrDefault().GetFirstValue();
            var cCodeDisplay = extrinsicObjectConfCode.Name.GetFirstValue();
            var ccodeValue = extrinsicObjectConfCode.NodeRepresentation;

            return new()
            {
                Code = ccodeValue,
                DisplayName = cCodeDisplay,
                CodeSystem = cCodeOid
            };
        }
        return null;
    }

    private static TS SetClinicalDocumentEffectiveTime(ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectSubmissionTime = extrinsicObject.GetSlots(Constants.Xds.SlotNames.CreationTime).FirstOrDefault()?.GetFirstValue();

        if (DateTimeOffset.TryParseExact(
                extrinsicObjectSubmissionTime?.Trim(),
                Constants.Hl7.Dtm.DtmFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var submissionTime))
        {
            return new()
            {
                EffectiveTime = submissionTime,
                Value = extrinsicObjectSubmissionTime
            };
        }

        return null;
    }

    private static CV SetClinicalDocumentTypeCode(ExtrinsicObjectType extrinsicObject)
    {
        var extrinsicObjectTypeCode = extrinsicObject.Classification
            .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.TypeCode);

        if (extrinsicObjectTypeCode is not null)
        {
            var cCodeOid = extrinsicObjectTypeCode.Slot.FirstOrDefault().GetFirstValue();
            var cCodeDisplay = extrinsicObjectTypeCode.Name.GetFirstValue();
            var ccodeValue = extrinsicObjectTypeCode.NodeRepresentation;

            return new()
            {
                Code = ccodeValue,
                DisplayName = cCodeDisplay,
                CodeSystem = cCodeOid
            };
        }
        return null;
    }

    static II SetClinicalDocumentId(ExtrinsicObjectType extrinsicObject)
    {
        var repositoryId = extrinsicObject.GetSlots(Constants.Xds.SlotNames.RepositoryUniqueId).FirstOrDefault()?.GetFirstValue();
        var documentUniqueId = extrinsicObject.ExternalIdentifier.FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.UniqueId);
        return new()
        {
            Root = repositoryId ?? string.Empty,
            Extension = documentUniqueId.Value ?? Guid.NewGuid().ToString(),
        };
    }





    /// <summary>
    /// Parse a CDA document and transform the content into a Registry object list IE for ITI-41.
    /// Will preserve the CDA document and its containing base64 document as part of the 
    /// </summary>
    //public IdentifiableType[] TransformCdaDocumentToRegistryObjectList(ClinicalDocument cdaDocument)
    //{
    //    var extrinsicObject = TransformCdaDocumentToExtrinsicObject(cdaDocument);
    //    var registryPackage = TransformCdaDocumentToRegistryPackage(cdaDocument);

    //    throw new NotImplementedException();
    //}

    //private ExtrinsicObjectType? TransformCdaDocumentToExtrinsicObject(ClinicalDocument cdaDocument)
    //{
    //    if (cdaDocument == null) return null;

    //    var extrinsicObject = new ExtrinsicObjectType();

    //    // Mimetype
    //    if (cdaDocument.Component?.NonXmlBody?.Text?.MediaType != null)
    //    {
    //        extrinsicObject.MimeType = cdaDocument.Component.NonXmlBody.Text.MediaType;
    //    }

    //    // Slot submissionTime
    //    if (cdaDocument.EffectiveTime.Value != null)
    //    {
    //        extrinsicObject.AddSlot(new SlotType(
    //            Constants.Xds.SlotNames.SubmissionTime,
    //            cdaDocument.EffectiveTime.Value
    //            ?? DateTime.UtcNow.ToString(Constants.Hl7.Dtm.DtmFormat)));
    //    }

    //    // Slot sourcePatientId
    //    var patientId = cdaDocument.RecordTarget.FirstOrDefault()?.PatientRole.Id.FirstOrDefault();
    //    if (patientId != null)
    //    {
    //        var sourcePatientId = new CX()
    //        {
    //            IdNumber = patientId.Extension,
    //            AssigningAuthority = new HD()
    //            {
    //                UniversalId = patientId.Root,
    //                UniversalIdType = Constants.Hl7.UniversalIdType.Iso
    //            }
    //        };
    //        var sourcePatientIdString = sourcePatientId.Serialize();
    //        extrinsicObject.AddSlot(new SlotType(Constants.Xds.SlotNames.SourcePatientId, sourcePatientIdString));
    //    }

    //    // Slot sourcePatientInfo
    //    var patient = cdaDocument.RecordTarget.FirstOrDefault()?.PatientRole;
    //    var cdaPatientId = cdaDocument.RecordTarget.FirstOrDefault()?.PatientRole.Id.FirstOrDefault();

    //    if (patient != null && cdaPatientId != null)
    //    {
    //        // PID-3
    //        var patientIdentifier = new CX()
    //        {
    //            IdNumber = cdaPatientId.Extension,
    //            AssigningAuthority = new HD()
    //            {
    //                UniversalId = cdaPatientId.Root,
    //                UniversalIdType = Constants.Hl7.UniversalIdType.Iso
    //            }
    //        };
    //        var patientIdPidString = $"PID-3|{patientIdentifier.Serialize()}";


    //        // PID-5
    //        var patientName = new XPN()
    //        {
    //            FamilyName = patient.Patient?.Name?.FirstOrDefault()?.Family?.FirstOrDefault()?.Value ?? "",
    //            GivenName = patient.Patient?.Name?.FirstOrDefault()?.Given?.FirstOrDefault()?.Value ?? ""
    //        };
    //        var patientNameString = $"PID-5|{patientName.Serialize()}";

    //        // PID-7

    //        // PID-8
    //        var patientGender = patient.Patient?.AdministrativeGenderCode?.Code switch
    //        {
    //            "1" => "M",
    //            "2" => "F",
    //            "9" => "O",
    //            "0" => "U",
    //            _ => "U"
    //        };
    //    }


    //    return extrinsicObject;
    //}

    //private RegistryPackageType? TransformCdaDocumentToRegistryPackage(ClinicalDocument cdaDocument)
    //{
    //    if (cdaDocument == null) return null;

    //    var registryPackage = new RegistryPackageType();

    //    return registryPackage;
    //}
}
