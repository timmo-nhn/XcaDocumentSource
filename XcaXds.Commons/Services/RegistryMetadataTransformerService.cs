using System.Globalization;
using System.Runtime.InteropServices;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public class RegistryMetadataTransformerService
{
    public DocumentReferenceDto TransformRegistryObjectsToDocumentEntryDto(ExtrinsicObjectType extrinsicObject, RegistryPackageType registryPackage, DocumentType document = null, AssociationType association = null)
    {
        var documentEntryDto = new DocumentReferenceDto();

        documentEntryDto.DocumentEntryMetadata = TransformExtrinsicObjectToDocumentEntryDto(extrinsicObject, registryPackage);
        documentEntryDto.SubmissionSetMetadata = TransformRegistryPackageToSubmissionSetDto(registryPackage);
        documentEntryDto.Association = TransformToAssociationDto(association, extrinsicObject, registryPackage);
        documentEntryDto.DocumentEntryDocument = new() { DocumentId = document.Id, Data = document.Value };

        return documentEntryDto;
    }

    private AssociationDto TransformToAssociationDto(AssociationType association, ExtrinsicObjectType extrinsicObject, RegistryPackageType registryPackage)
    {
        var associationDto = new AssociationDto();

        associationDto.SourceObject = association?.SourceObject ?? registryPackage.Id;
        associationDto.TargetObject = association?.TargetObject ?? extrinsicObject.Id;
        associationDto.AssociationType = association?.AssociationTypeData ?? Constants.Xds.AssociationType.HasMember;
        associationDto.SubmissionSetStatus = association?.GetFirstSlot(Constants.Xds.SlotNames.SubmissionSetStatus)?.GetFirstValue() ?? "Original";
        return associationDto;
    }

    private SubmissionSetDto TransformRegistryPackageToSubmissionSetDto(RegistryPackageType registryPackage)
    {
        var submissionSet = new SubmissionSetDto();

        submissionSet.Author = GetAuthorFromRegistryPackage(registryPackage);
        submissionSet.AvailabilityStatus = registryPackage.Status;
        submissionSet.ContentTypeCode = GetContentTypeCodeFromRegistryPackage(registryPackage);
        submissionSet.HomeCommunityId = registryPackage.Home;
        submissionSet.Id = registryPackage.Id;
        submissionSet.PatientId = GetPatientIdFromRegistryPackage(registryPackage);
        submissionSet.SourceId = GetSourceIdFromRegistryPackage(registryPackage);
        submissionSet.SubmissionTime = GetSubmissionTimeFromRegistryPackage(registryPackage);
        submissionSet.Title = GetTitleFromRegistryPackage(registryPackage);
        submissionSet.UniqueId = GetUniqueIdFromRegistryPackage(registryPackage);


        return submissionSet;
    }

    private string? GetUniqueIdFromRegistryPackage(RegistryPackageType registryPackage)
    {
        return registryPackage.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.UniqueId)?.Value;
    }

    private string? GetTitleFromRegistryPackage(RegistryPackageType registryPackage)
    {
        return registryPackage.Name.GetFirstValue();
    }

    private DateTime? GetSubmissionTimeFromRegistryPackage(RegistryPackageType registryPackage)
    {
        var dateValue = registryPackage.GetFirstSlot(Constants.Xds.SlotNames.CreationTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private string? GetSourceIdFromRegistryPackage(RegistryPackageType registryPackage)
    {
        return registryPackage.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.SourceId)?.Value;
    }

    private CodedValue? GetPatientIdFromRegistryPackage(RegistryPackageType registryPackage)
    {
        var patientIdExtIder = registryPackage.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.PatientId).Value;

        if (patientIdExtIder != null)
        {
            var patientIdValue = Hl7Object.Parse<CX>(patientIdExtIder);

            return new()
            {
                Code = patientIdValue.IdNumber,
                CodeSystem = patientIdValue.AssigningAuthority.UniversalId
            };
        }

        return null;
    }

    private CodedValue? GetContentTypeCodeFromRegistryPackage(RegistryPackageType registryPackage)
    {
        var contentTypeClass = registryPackage.GetFirstClassification(Constants.Xds.Uuids.SubmissionSet.ContentTypeCode);

        if (contentTypeClass != null)
        {
            return new()
            {
                Code = contentTypeClass?.NodeRepresentation ?? string.Empty,
                CodeSystem = contentTypeClass?.GetFirstSlot()?.GetFirstValue() ?? string.Empty,
                DisplayName = contentTypeClass?.Name?.GetFirstValue() ?? string.Empty
            };
        }
        return null;
    }


    private Author GetAuthorFromRegistryPackage(RegistryPackageType registryPackage)
    {
        var authorClassification = registryPackage.GetFirstClassification(Constants.Xds.Uuids.SubmissionSet.Author);

        var author = new Author();
        author.Organization = GetAuthorOrganizationFromClassification(authorClassification);
        author.Department = GetAuthorDepartmentFromClassification(authorClassification);
        author.Person = GetAuthorPersonFromClassification(authorClassification);
        author.Role = GetAuthorRoleFromClassificaiton(authorClassification);
        author.Speciality = GetAuthorSpecialityFromClassification(authorClassification);

        return author;

    }

    private DocumentEntryDto TransformExtrinsicObjectToDocumentEntryDto(ExtrinsicObjectType extrinsicObject, RegistryPackageType registryObjects)
    {
        var documentMetadata = new DocumentEntryDto();

        documentMetadata.Author = GetAuthorFromExtrinsicObject(extrinsicObject);
        documentMetadata.AvailabilityStatus = extrinsicObject.Status;
        documentMetadata.ClassCode = GetClassCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.ConfidentialityCode = GetConfidentialityCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.CreationTime = GetCreationTimeFromExtrinsicObject(extrinsicObject);
        documentMetadata.DocumentUniqueId = GetDocumentUniqueIdFromExtrinsicObject(extrinsicObject);
        documentMetadata.EventCodeList = GetEventCodeListFromExtrinsicObject(extrinsicObject);
        documentMetadata.FormatCode = GetFormatCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.Hash = GetHashFromExtrinsicObject(extrinsicObject);
        documentMetadata.HealthCareFacilityTypeCode = GetHealthCareFacilityTypeCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.HomeCommunityId = extrinsicObject.Home;
        documentMetadata.Id = extrinsicObject.Id;
        documentMetadata.LanguageCode = GetLanguageCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.LegalAuthenticator = GetLegalAuthenticatorFromExtrinsicObject(extrinsicObject);
        documentMetadata.MimeType = extrinsicObject.MimeType;
        documentMetadata.ObjectType = extrinsicObject.ObjectType;
        documentMetadata.PatientId = GetSourcePatientIdFromExtrinsicObject(extrinsicObject);
        documentMetadata.PracticeSettingCode = GetPracticeSettingCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.RepositoryUniqueId = GetRepositoryUniqueIdFromExtrinsicObject(extrinsicObject);
        documentMetadata.Size = GetSizeFromExtrinsicObject(extrinsicObject);
        documentMetadata.ServiceStartTime = GetServiceStartTimeFromExtrinsicObject(extrinsicObject);
        documentMetadata.ServiceStopTime = GetServiceStopTimeFromExtrinsicObject(extrinsicObject);
        documentMetadata.SourcePatientInfo = GetSourcePatientInfoFromExtrinsicObject(extrinsicObject);
        documentMetadata.Title = GetTitleFromExtrinsicObject(extrinsicObject);

        return documentMetadata;
    }

    private string GetTitleFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.Name.GetFirstValue();
    }

    private SourcePatientInfo GetSourcePatientInfoFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var sourcePatientInfo = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientInfo)?.GetValues();

        if (sourcePatientInfo != null)
        {
            var patientId = Hl7Object.Parse<CX>(sourcePatientInfo?.FirstOrDefault(s => s.Contains("PID-3"))?.Split("PID-3|")?.LastOrDefault());
            var name = Hl7Object.Parse<XPN>(sourcePatientInfo?.FirstOrDefault(s => s.Contains("PID-5"))?.Split("PID-5|")?.LastOrDefault());
            var birthTime = sourcePatientInfo.FirstOrDefault(s => s.Contains("PID-7"))?.Split("PID-7|").LastOrDefault();
            var gender = sourcePatientInfo.FirstOrDefault(s => s.Contains("PID-8"))?.Split("PID-8|").LastOrDefault();

            return new()
            {
                BirthTime = birthTime == null ? null : DateTime.ParseExact(birthTime, Constants.Hl7.Dtm.DtmYmdFormat, CultureInfo.InvariantCulture),
                FamilyName = name.FamilyName,
                GivenName = name.GivenName,
                Gender = gender ?? "U",
                PatientId = patientId?.IdNumber == null ? null : new()
                {
                    Id = patientId.IdNumber,
                    System = patientId.AssigningFacility.UniversalId ?? patientId.AssigningAuthority.UniversalId,
                }
            };
        }

        return null;
    }

    private string GetSizeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.Size)?.GetFirstValue();
    }

    private DateTime? GetServiceStopTimeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var dateValue = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.ServiceStopTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private DateTime? GetServiceStartTimeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var dateValue = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.ServiceStartTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private string GetRepositoryUniqueIdFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.RepositoryUniqueId)?.GetFirstValue();
    }

    private CodedValue GetPracticeSettingCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var practiceSettingClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode);

        if (practiceSettingClassification != null)
        {
            return MapToCodedValue(practiceSettingClassification);
        }

        return null;
    }

    private CodedValue? GetSourcePatientIdFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var sourcePatientId = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientId)?.GetFirstValue();
        var patIdPid = Hl7Object.Parse<CX>(sourcePatientId);

        if (patIdPid != null)
        {
            return new()
            {
                Code = patIdPid.IdNumber,
                DisplayName = patIdPid.AssigningAuthority.UniversalId
            };
        }

        return null;

    }

    private LegalAuthenticator? GetLegalAuthenticatorFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var legalAuthenticator = new LegalAuthenticator();

        var legalAuthenticatorSlot = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.LegalAuthenticator)?.GetFirstValue();

        var legAuthXcn = Hl7Object.Parse<XCN>(legalAuthenticatorSlot);

        if (legAuthXcn != null)
        {
            legalAuthenticator.FamilyName = legAuthXcn.FamilyName;
            legalAuthenticator.GivenName = legAuthXcn.GivenName;
            legalAuthenticator.Id = legAuthXcn.PersonIdentifier;
            legalAuthenticator.IdSystem = legAuthXcn.AssigningAuthority.UniversalId;

            return legalAuthenticator;
        }

        return null;



    }

    private string GetLanguageCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.LanguageCode)?.GetFirstValue();
    }

    private string? GetHashFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.Hash)?.GetFirstValue();
    }

    private CodedValue? GetHealthCareFacilityTypeCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var healthcareTypeCodeClassificaiton = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.HealthCareFacilityTypeCode);

        if (healthcareTypeCodeClassificaiton != null)
        {
            return MapToCodedValue(healthcareTypeCodeClassificaiton);
        }

        return null;
    }

    private CodedValue? GetFormatCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var formatCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.FormatCode);

        if (formatCodeClassification != null)
        {
            return MapToCodedValue(formatCodeClassification);
        }

        return null;
    }

    private CodedValue? GetEventCodeListFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var eventCodeListClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.EventCodeList);

        if (eventCodeListClassification != null)
        {
            return MapToCodedValue(eventCodeListClassification);
        }

        return null;
    }

    private string? GetDocumentUniqueIdFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var documentUniqueId = extrinsicObject.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId).Value;
        return documentUniqueId;
    }

    private DateTime? GetCreationTimeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var dateValue = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.CreationTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private CodedValue GetConfidentialityCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var confCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode);

        if (confCodeClassification != null)
        {
            return MapToCodedValue(confCodeClassification);
        }

        return null;
    }

    private CodedValue GetClassCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var classCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ClassCode);

        return MapToCodedValue(classCodeClassification);
    }

    private CodedValue MapToCodedValue(ClassificationType classification)
    {
        return new()
        {
            Code = classification?.NodeRepresentation ?? string.Empty,
            CodeSystem = classification?.GetFirstSlot()?.GetFirstValue() ?? string.Empty,
            DisplayName = classification?.Name?.GetFirstValue() ?? string.Empty
        };
    }

    private Author GetAuthorFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {

        var authorClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.Author);

        var author = new Author();
        author.Organization = GetAuthorOrganizationFromClassification(authorClassification);
        author.Department = GetAuthorDepartmentFromClassification(authorClassification);
        author.Person = GetAuthorPersonFromClassification(authorClassification);
        author.Role = GetAuthorRoleFromClassificaiton(authorClassification);
        author.Speciality = GetAuthorSpecialityFromClassification(authorClassification);

        return author;
    }

    private AuthorPerson? GetAuthorPersonFromClassification(ClassificationType authorClassification)
    {
        var authorPerson = new AuthorPerson();

        var authorPersonXcn = authorClassification
            .GetSlots(Constants.Xds.SlotNames.AuthorPerson)
            .GetValues()
            .Select(asl => Hl7Object.Parse<XCN>(asl)).FirstOrDefault();

        // Resolve whether authorPerson is stored as a simple string or as XCN
        if (authorPersonXcn.PersonIdentifier != null && authorPersonXcn.GivenName == null)
        {
            var nameParts = authorPersonXcn?.PersonIdentifier.Split(' ');
            authorPerson.FirstName = nameParts?.FirstOrDefault();
            authorPerson.LastName = string.Join(' ', nameParts.Skip(1));
            return authorPerson;
        }
        else if (authorPersonXcn != null)
        {
            authorPerson.FirstName = authorPersonXcn.GivenName;
            authorPerson.LastName = authorPersonXcn.FamilyName;
            authorPerson.Id = authorPersonXcn.PersonIdentifier;
            authorPerson.AssigningAuthority = authorPersonXcn.AssigningAuthority?.UniversalId;
            return authorPerson;
        }

        return null;
    }

    private AuthorOrganization? GetAuthorDepartmentFromClassification(ClassificationType authorClassification)
    {
        var authorOrganization = new AuthorOrganization();

        var authorSlotXon = authorClassification
            .GetSlots(Constants.Xds.SlotNames.AuthorInstitution)
            .GetValues()
            .Select(asl => Hl7Object.Parse<XON>(asl))
            .ToArray();

        // Find organization XON here aswell to ensure we dont double-register stuff
        var organization = authorSlotXon
            .FirstOrDefault(asXon =>
                (asXon?.AssigningFacility?.UniversalId != null && asXon.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg)) ||
                (asXon?.AssigningAuthority?.UniversalId != null && asXon.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotXon.LastOrDefault();


        // Find the XON object where assigningAuthority is NOT brreg(ie. empty, OID for department or other OID).
        // If none is found, take the first in the XON list
        var department = authorSlotXon
            .FirstOrDefault(asXon =>
                (asXon?.AssigningFacility?.UniversalId != null && !asXon.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg)) ||
                (asXon?.AssigningAuthority?.UniversalId != null && !asXon.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotXon.FirstOrDefault();

        if (department != null && department?.OrganizationIdentifier != organization?.OrganizationIdentifier)
        {
            return new()
            {
                AssigningAuthority = department?.AssigningAuthority?.UniversalId ?? department?.AssigningFacility?.UniversalId ?? string.Empty,
                Id = department?.OrganizationIdentifier ?? string.Empty,
                OrganizationName = department?.OrganizationName ?? string.Empty
            };
        }

        return null;
    }

    private AuthorOrganization? GetAuthorOrganizationFromClassification(ClassificationType authorClassification)
    {
        var authorOrganization = new AuthorOrganization();

        var authorSlotXon = authorClassification
            .GetSlots(Constants.Xds.SlotNames.AuthorInstitution)
            .GetValues()
            .Select(asl => Hl7Object.Parse<XON>(asl))
            .ToArray();

        // Find the XON object where assigningAuthority is NOT brreg.
        // If none is found, take the last in the XON list
        var organization = authorSlotXon
            .FirstOrDefault(asXon =>
                (asXon?.AssigningFacility?.UniversalId != null && asXon.AssigningFacility.UniversalId.Contains(Constants.Oid.Brreg)) ||
                (asXon?.AssigningAuthority?.UniversalId != null && asXon.AssigningAuthority.UniversalId.Contains(Constants.Oid.Brreg)))
            ?? authorSlotXon.LastOrDefault();

        if (organization != null)
        {
            authorOrganization.AssigningAuthority = organization?.AssigningAuthority?.UniversalId ?? organization?.AssigningFacility?.UniversalId ?? string.Empty;
            authorOrganization.Id = organization?.OrganizationIdentifier ?? string.Empty;
            authorOrganization.OrganizationName = organization?.OrganizationName ?? string.Empty;
            return authorOrganization;
        }

        return null;
    }

    private CodedValue? GetAuthorSpecialityFromClassification(ClassificationType authorClassification)
    {
        var authorSpeciality = authorClassification
            .GetSlots(Constants.Xds.SlotNames.AuthorSpecialty)
            .GetValues()
            .Select(rol => Hl7Object.Parse<CX>(rol)).FirstOrDefault();
        if (authorSpeciality != null)
        {
            return new()
            {
                Code = authorSpeciality?.IdNumber,
                CodeSystem = authorSpeciality?.AssigningAuthority?.UniversalId,
            };
        }

        return null;
    }

    private CodedValue? GetAuthorRoleFromClassificaiton(ClassificationType classification)
    {
        var authorRole = classification
            .GetSlots(Constants.Xds.SlotNames.AuthorRole)
            .GetValues()
            .Select(rol => Hl7Object.Parse<CX>(rol)).FirstOrDefault();

        if (authorRole != null)
        {
            return new()
            {
                Code = authorRole?.IdNumber,
                CodeSystem = authorRole?.AssigningAuthority?.UniversalId,
            };
        }

        return null;

    }

    public IdentifiableType[] TransformDocumentEntryDtoToRegistryObjects(DocumentReferenceDto documentReference)
    {
        var registryObjectList = new List<IdentifiableType>();

        var extrinsicObject = GetExtrinsicObjectFromDocumentReferenceDto(documentReference.DocumentEntryMetadata);
        var registryPackage = GetRegistryPackageFromDocumentReferenceDto(documentReference.SubmissionSetMetadata);
        var association = GetAssociationFromAssociationDto(documentReference.Association);

        registryObjectList.Add(extrinsicObject);

        return registryObjectList.ToArray();
    }

    private ExtrinsicObjectType GetExtrinsicObjectFromDocumentReferenceDto(DocumentEntryDto documentEntryMetadata)
    {
        var extrinsicObject = new ExtrinsicObjectType();

        GetAuthorClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetAvailabilityStatusFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetClassCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetCreationTimeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetConfidentialityCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetEventCodeListClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetFormatCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetHashSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetHealthCareFacilityTypeCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetHomeCommunityIdFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetIdFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetLanguageCodeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetLegalAuthenticatorSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetMimeTypeFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetObjectTypeFromDocumentEntryDto(documentEntryMetadata, extrinsicObject);
        GetSourcePatientIdSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetServiceStartTimeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetServiceStopTimeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetSourcePatientInfoSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetRepositoryUniqueIdSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetSizeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetNameLocalizedStringFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);


        return extrinsicObject;
    }

    private void GetSizeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.Size != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.Size, [documentEntryMetadata.Size]);
        }
    }

    private void GetRepositoryUniqueIdSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var repositoryUniqueId = documentEntryMetadata.RepositoryUniqueId;
        if (repositoryUniqueId != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.RepositoryUniqueId, [repositoryUniqueId]);
        }
    }

    private void GetSourcePatientInfoSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var sourcePatientIdSlot = new SlotType();
        var sourcePatientInfo = documentEntryMetadata.SourcePatientInfo;


        if (sourcePatientInfo?.PatientId != null)
        {
            var patientId = new CX()
            {
                IdNumber = sourcePatientInfo?.PatientId?.Id,
                AssigningAuthority = new()
                {
                    UniversalId = sourcePatientInfo?.PatientId?.System,
                    NamespaceId = Constants.Hl7.UniversalIdType.Iso
                }
            };

            sourcePatientIdSlot.AddValue($"PID-3|{patientId.Serialize()}");
        }


        if (sourcePatientInfo?.FamilyName != null)
        {
            var lastNameParts = sourcePatientInfo.FamilyName?.Split(' ');

            var sourcePatientXcn = new XPN()
            {
                GivenName = sourcePatientInfo.GivenName ?? string.Empty,
                FamilyName = sourcePatientInfo.FamilyName ?? string.Empty,
            };

            sourcePatientIdSlot.AddValue($"PID-5|{sourcePatientXcn.Serialize()}");
        }


        if (sourcePatientInfo?.BirthTime != null && sourcePatientInfo?.BirthTime.Value != null)
        {
            sourcePatientIdSlot.AddValue($"PID-7|{sourcePatientInfo?.BirthTime.Value.ToString(Constants.Hl7.Dtm.DtmYmdFormat)}");
        }


        if (sourcePatientInfo?.Gender != null)
        {
            sourcePatientIdSlot.AddValue($"PID-8|{sourcePatientInfo.Gender}");
        }

    }

    private void GetServiceStopTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var serviceStartTime = documentEntryMetadata.ServiceStopTime;
        if (serviceStartTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.ServiceStopTime, [serviceStartTime.Value.ToString()]);
        }
    }

    private void GetServiceStartTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var serviceStartTime = documentEntryMetadata.ServiceStartTime;
        if (serviceStartTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.ServiceStartTime, [serviceStartTime.Value.ToString()]);
        }
    }

    private void GetNameLocalizedStringFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.Title != null)
        {
            extrinsicObject.Name.LocalizedString = [new() { Value = documentEntryMetadata.Title }];
        }
    }

    private void GetMimeTypeFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        extrinsicObject.MimeType = documentEntryMetadata.MimeType;
    }

    private void GetLegalAuthenticatorSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var legalAuthenticator = documentEntryMetadata.LegalAuthenticator;
        if (legalAuthenticator != null)
        {
            var lastNameParts = legalAuthenticator.FamilyName?.Split(' ');
            var middleName = lastNameParts?.FirstOrDefault();
            var lastName = string.Join(" ", lastNameParts?.Skip(1));
            var legalAuthXcn = new XCN()
            {
                PersonIdentifier = legalAuthenticator.Id,
                GivenName = legalAuthenticator.GivenName,
                MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                FamilyName = lastName,
                AssigningAuthority = new()
                {
                    UniversalId = legalAuthenticator.IdSystem,
                    NamespaceId = Constants.Hl7.UniversalIdType.Iso
                }
            };

            extrinsicObject.AddSlot(Constants.Xds.SlotNames.LegalAuthenticator, [legalAuthXcn.Serialize()]);
        }
    }

    private void GetHomeCommunityIdFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var homeCommunityId = documentEntryMetadata.HomeCommunityId;
        if (homeCommunityId != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.HomeCommunityId, [homeCommunityId]);
        }
    }

    private void GetHealthCareFacilityTypeCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var healthcareFacilityTypeCode = documentEntryMetadata.HealthCareFacilityTypeCode;
        var healthcareFacilityTypeCodeClassification = MapToClassification(healthcareFacilityTypeCode);

        if (healthcareFacilityTypeCodeClassification != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, healthcareFacilityTypeCodeClassification];
        }
    }

    private void GetFormatCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var formatCode = documentEntryMetadata.ClassCode;
        var formatCodeClassification = MapToClassification(formatCode);

        if (formatCodeClassification != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, formatCodeClassification];
        }
    }

    private void GetEventCodeListClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var eventCode = documentEntryMetadata.ClassCode;
        var eventCodeClassification = MapToClassification(eventCode);

        if (eventCodeClassification != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, eventCodeClassification];
        }
    }

    private void GetConfidentialityCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var confCode = documentEntryMetadata.ConfidentialityCode;
        var confCodeClassification = MapToClassification(confCode);

        if (confCodeClassification != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, confCodeClassification];
        }
    }

    private ClassificationType MapToClassification(CodedValue? confCode)
    {
        if (confCode != null)
        {
            return new()
            {
                NodeRepresentation = confCode.Code,
                Name = new() { LocalizedString = [new() { Value = confCode.DisplayName }] },
                Slot = [new() { Name = Constants.Xds.SlotNames.CodingScheme, ValueList = new() { Value = [confCode.CodeSystem] } }]
            };
        }

        return null;
    }

    private void GetClassCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var classCodeClassification = new ClassificationType();
        var classCode = documentEntryMetadata.ClassCode;

        if (classCode != null)
        {
            classCodeClassification.NodeRepresentation = classCode.Code;
            classCodeClassification.Name.LocalizedString = [new() { Value = classCode.Code }];
            classCodeClassification.AddSlot(Constants.Xds.SlotNames.CodingScheme, [classCode.CodeSystem]);
        }
    }

    private void GetHashSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.Hash == null) return;

        extrinsicObject.AddSlot(Constants.Xds.SlotNames.Hash, [documentEntryMetadata.Hash]);
    }

    private void GetAuthorClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var authorClassification = new ClassificationType();

        GetAuthorPersonSlotFromAuthor(authorClassification, documentEntryMetadata);
        GetAuthorInstitutionSlotFromAuthor(authorClassification, documentEntryMetadata);
        GetAuthorRoleSlotFromAuthor(authorClassification, documentEntryMetadata);
        GetAuthorSpecialitySlotFromAuthor(authorClassification, documentEntryMetadata);

        extrinsicObject.Classification = [.. extrinsicObject.Classification, authorClassification];
    }

    private void GetAuthorSpecialitySlotFromAuthor(ClassificationType classification, Author documentAuthor)
    {
        var authorSpeciality = documentAuthor?.Speciality;

        if (authorSpeciality == null) return;

        var authorSpecialityCx = new CX()
        {
            IdNumber = authorSpeciality.Code,
            AssigningAuthority = new HD()
            {
                UniversalId = authorSpeciality.CodeSystem,
                NamespaceId = Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorSpecialty, [authorSpecialityCx.Serialize()]);
    }

    private void GetAuthorRoleSlotFromAuthor(ClassificationType classification, DocumentEntryDto documentEntryMetadata)
    {
        var authorRole = documentEntryMetadata?.Author?.Role;
        if (authorRole == null) return;

        var authorRoleCx = new CX()
        {
            IdNumber = authorRole.Code,
            AssigningAuthority = new HD()
            {
                UniversalId = authorRole.CodeSystem,
                NamespaceId = Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorRole, [authorRoleCx.Serialize()]);
    }

    private void GetAuthorInstitutionSlotFromAuthor(ClassificationType classification, DocumentEntryDto documentEntryMetadata)
    {
        var authorInstitution = documentEntryMetadata?.Author;
        if (authorInstitution == null) return;

        var authorInstitutionSlot = new SlotType()
        {
            Name = Constants.Xds.SlotNames.AuthorInstitution
        };

        if (authorInstitution.Organization != null)
        {
            var org = new XON()
            {
                OrganizationName = authorInstitution.Organization.OrganizationName,
                OrganizationIdentifier = authorInstitution.Organization.Id,
                AssigningAuthority = new()
                {
                    UniversalId = authorInstitution.Organization.AssigningAuthority,
                    NamespaceId = Constants.Hl7.UniversalIdType.Iso
                }
            };

            authorInstitutionSlot.ValueList.Value = [org.Serialize()];
        }

        if (authorInstitution.Department != null)
        {
            var dpt = new XON()
            {
                OrganizationName = authorInstitution.Department.OrganizationName,
                OrganizationIdentifier = authorInstitution.Department.Id,
                AssigningAuthority = new()
                {
                    UniversalId = authorInstitution.Department.AssigningAuthority,
                    NamespaceId = Constants.Hl7.UniversalIdType.Iso
                }
            };

            authorInstitutionSlot.ValueList.Value = [.. authorInstitutionSlot.ValueList.Value, dpt.Serialize()];
        }

        classification.AddSlot(authorInstitutionSlot);
    }

    private void GetAuthorPersonSlotFromAuthor(ClassificationType classification, DocumentEntryDto documentEntryMetadata)
    {
        var authorPerson = documentEntryMetadata?.Author?.Person;
        if (authorPerson != null)
        {
            var lastNameParts = authorPerson.LastName?.Split(' ');
            var middleName = lastNameParts?.FirstOrDefault();
            var lastName = string.Join(" ", lastNameParts?.Skip(1));
            var authorXcn = new XCN()
            {
                PersonIdentifier = authorPerson.Id,
                GivenName = authorPerson.FirstName,
                MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                FamilyName = lastName,
                AssigningAuthority = new()
                {
                    UniversalId = authorPerson.AssigningAuthority,
                    NamespaceId = Constants.Hl7.UniversalIdType.Iso
                }
            };

            classification.AddSlot(Constants.Xds.SlotNames.AuthorPerson, [authorXcn.Serialize()]);
        }
    }

    private static void GetAvailabilityStatusFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.AvailabilityStatus != null)
        {
            extrinsicObject.Status = documentEntryMetadata.AvailabilityStatus;
        }
    }

    private static void GetObjectTypeFromDocumentEntryDto(DocumentEntryDto documentEntryMetadata, ExtrinsicObjectType extrinsicObject)
    {
        if (documentEntryMetadata?.ObjectType != null)
        {
            extrinsicObject.ObjectType = documentEntryMetadata.ObjectType;
        }
    }

    private static void GetIdFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.Id != null)
        {
            extrinsicObject.Id = documentEntryMetadata.Id;
        }
    }

    private void GetSourcePatientIdSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.PatientId?.Code != null)
        {
            var patientId = new CX()
            {
                IdNumber = documentEntryMetadata.PatientId.Code,
                AssigningAuthority = new() { UniversalId = documentEntryMetadata.PatientId.CodeSystem }
            };
        }
    }

    private void GetLanguageCodeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.LanguageCode != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.LanguageCode, [documentEntryMetadata.LanguageCode]);
        }
    }

    private void GetCreationTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.CreationTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.CreationTime, [documentEntryMetadata.CreationTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private RegistryPackageType GetRegistryPackageFromDocumentReferenceDto(SubmissionSetDto submissionSetMetadata)
    {
        var registryPackage = new RegistryPackageType();

        GetAuthorClassificationFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetAvailabilitystatusFromSubmissionSetDto(registryPackage,submissionSetMetadata);
        GetContentTypeCodeFromSubmissionSetDto(registryPackage,submissionSetMetadata);
        GetSubmissionTimeFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetHomeCommunityIdFromSubmissionSetDto(registryPackage,submissionSetMetadata);
        registryPackage.Id = submissionSetMetadata.Id;
        GetPatientIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetSourceIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetSubmissionTimeFromSubmissionSetDto(registryPackage,submissionSetMetadata);
        GetNameLocalizedStringFromSubmissionSetDto(registryPackage,submissionSetMetadata);
        GetUniqueIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);

        return registryPackage;
    }

    private void GetAuthorClassificationFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var authorClassification = new ClassificationType();

        GetAuthorInstitutionSlotFromAuthor(authorClassification, submissionSetMetadata);

        var authorPerson = submissionSetMetadata.Author.Person;

        if (authorPerson != null)
        {
            var lastNameParts = authorPerson.LastName?.Split(' ');
            var middleName = lastNameParts?.FirstOrDefault();
            var lastName = string.Join(" ", lastNameParts?.Skip(1));
            var authorXcn = new XCN()
            {
                PersonIdentifier = authorPerson.Id,
                GivenName = authorPerson.FirstName,
                MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                FamilyName = lastName,
                AssigningAuthority = new()
                {
                    UniversalId = authorPerson.AssigningAuthority,
                    NamespaceId = Constants.Hl7.UniversalIdType.Iso
                }
            };

            authorClassification.AddSlot(Constants.Xds.SlotNames.AuthorPerson, [authorXcn.Serialize()]);
        }



    }

    private AssociationType GetAssociationFromAssociationDto(AssociationDto association)
    {
        throw new NotImplementedException();
    }
}
