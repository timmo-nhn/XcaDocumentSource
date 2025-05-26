using System.Globalization;
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

        associationDto.Id = association.Id;
        associationDto.SourceObject = association?.SourceObject ?? registryPackage.Id;
        associationDto.TargetObject = association?.TargetObject ?? extrinsicObject.Id;
        associationDto.AssociationType = association?.AssociationTypeData ?? Constants.Xds.AssociationType.HasMember;
        associationDto.SubmissionSetStatus = association?.GetFirstSlot(Constants.Xds.SlotNames.SubmissionSetStatus)?.GetFirstValue() ?? "Original";
        return associationDto;
    }

    private SubmissionSetDto TransformRegistryPackageToSubmissionSetDto(RegistryPackageType registryPackage)
    {
        var submissionSetDto = new SubmissionSetDto();

        submissionSetDto.Author = GetAuthorFromRegistryPackage(registryPackage);
        submissionSetDto.AvailabilityStatus = registryPackage.Status;
        submissionSetDto.ContentTypeCode = GetContentTypeCodeFromRegistryPackage(registryPackage);
        submissionSetDto.HomeCommunityId = registryPackage.Home;
        submissionSetDto.Id = registryPackage.Id;
        submissionSetDto.PatientId = GetPatientIdFromRegistryPackage(registryPackage);
        submissionSetDto.SourceId = GetSourceIdFromRegistryPackage(registryPackage);
        submissionSetDto.SubmissionTime = GetSubmissionTimeFromRegistryPackage(registryPackage);
        submissionSetDto.Title = GetTitleFromRegistryPackage(registryPackage);
        submissionSetDto.UniqueId = GetUniqueIdFromRegistryPackage(registryPackage);


        return submissionSetDto;
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
        var contentTypeCode = registryPackage.GetFirstClassification(Constants.Xds.Uuids.SubmissionSet.ContentTypeCode);

        if (contentTypeCode != null)
        {
            return new()
            {
                Code = contentTypeCode?.NodeRepresentation ?? string.Empty,
                CodeSystem = contentTypeCode?.GetFirstSlot()?.GetFirstValue() ?? string.Empty,
                DisplayName = contentTypeCode?.Name?.GetFirstValue() ?? string.Empty
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
        documentMetadata.TypeCode = GetTypeCodeFromExtrinsicObject(extrinsicObject);

        return documentMetadata;
    }

    private CodedValue? GetTypeCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var typeCode = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.TypeCode);

        if (typeCode != null)
        {
            return MapClassificationToCodedValue(typeCode);
        }
        return null;
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
            return MapClassificationToCodedValue(practiceSettingClassification);
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
                CodeSystem = patIdPid.AssigningAuthority.UniversalId
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
            return MapClassificationToCodedValue(healthcareTypeCodeClassificaiton);
        }

        return null;
    }

    private CodedValue? GetFormatCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var formatCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.FormatCode);

        if (formatCodeClassification != null)
        {
            return MapClassificationToCodedValue(formatCodeClassification);
        }

        return null;
    }

    private CodedValue? GetEventCodeListFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var eventCodeListClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.EventCodeList);

        if (eventCodeListClassification != null)
        {
            return MapClassificationToCodedValue(eventCodeListClassification);
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
            return MapClassificationToCodedValue(confCodeClassification);
        }

        return null;
    }

    private CodedValue GetClassCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var classCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ClassCode);

        return MapClassificationToCodedValue(classCodeClassification);
    }

    private CodedValue MapClassificationToCodedValue(ClassificationType classification)
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
        registryObjectList.Add(registryPackage);
        registryObjectList.Add(association);

        return registryObjectList.ToArray();
    }

    private ExtrinsicObjectType GetExtrinsicObjectFromDocumentReferenceDto(DocumentEntryDto documentEntryMetadata)
    {
        var extrinsicObject = new ExtrinsicObjectType()
        {
            Classification = [],
            ExternalIdentifier = []
        };
        extrinsicObject.Id = documentEntryMetadata.Id;
        GetAuthorClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetAvailabilityStatusFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetClassCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetConfidentialityCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetCreationTimeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetEventCodeListClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetFormatCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetHashSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetHealthCareFacilityTypeCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetHomeCommunityIdFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetLanguageCodeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetLegalAuthenticatorSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetMimeTypeFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetObjectTypeFromDocumentEntryDto(documentEntryMetadata, extrinsicObject);
        GetPatientIdExternalIdentifierFromDocumentEntryDto(documentEntryMetadata, extrinsicObject);
        GetPracticeSettingCodeFromDocumentEntryDto(documentEntryMetadata, extrinsicObject);
        GetRepositoryUniqueIdSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetServiceStartTimeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetServiceStopTimeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetSizeSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetSourcePatientIdSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetSourcePatientInfoSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetTitleNameLocalizedStringFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetTypeCodeClassificationFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetUniqueIdExternalIdentifierFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);

        return extrinsicObject;
    }

    private void GetUniqueIdExternalIdentifierFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId, documentEntryMetadata.UniqueId);
        if (externalIdentifier != null)
        {
            extrinsicObject.ExternalIdentifier = [.. extrinsicObject.ExternalIdentifier, externalIdentifier];
        }
    }

    private void GetTypeCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var typeCodeClass = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.TypeCode, documentEntryMetadata.TypeCode);
        if (typeCodeClass != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, typeCodeClass];
        }
    }

    private void GetPracticeSettingCodeFromDocumentEntryDto(DocumentEntryDto documentEntryMetadata, ExtrinsicObjectType extrinsicObject)
    {
        var patientIdExtId = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode, documentEntryMetadata.PracticeSettingCode);
        if (patientIdExtId != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, patientIdExtId];
        }
    }

    private void GetPatientIdExternalIdentifierFromDocumentEntryDto(DocumentEntryDto documentEntryMetadata, ExtrinsicObjectType extrinsicObject)
    {
        extrinsicObject.ExternalIdentifier ??= [];

        var patientIdExtId = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.PatientId, documentEntryMetadata.PatientId);
        if (patientIdExtId != null)
        {
            extrinsicObject.ExternalIdentifier = [.. extrinsicObject.ExternalIdentifier, patientIdExtId];
        }
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
        var sourcePatientIdSlot = new SlotType()
        {
            Name = Constants.Xds.SlotNames.SourcePatientInfo
        };
        var sourcePatientInfo = documentEntryMetadata.SourcePatientInfo;


        if (sourcePatientInfo?.PatientId != null)
        {
            var patientId = new CX()
            {
                IdNumber = sourcePatientInfo?.PatientId?.Id,
                AssigningAuthority = new()
                {
                    UniversalId = sourcePatientInfo?.PatientId?.System,
                    UniversalIdType = sourcePatientInfo?.PatientId?.System == null ? null : Constants.Hl7.UniversalIdType.Iso
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

        extrinsicObject.AddSlot(sourcePatientIdSlot);
    }

    private void GetServiceStopTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var serviceStartTime = documentEntryMetadata.ServiceStopTime;
        if (serviceStartTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.ServiceStopTime, [serviceStartTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private void GetServiceStartTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var serviceStartTime = documentEntryMetadata.ServiceStartTime;
        if (serviceStartTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.ServiceStartTime, [serviceStartTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private void GetTitleNameLocalizedStringFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.Title != null)
        {
            extrinsicObject.Name = new() { LocalizedString = [new() { Value = documentEntryMetadata.Title }] };
        }
    }

    private void GetMimeTypeFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (!string.IsNullOrWhiteSpace(documentEntryMetadata.MimeType))
        {
            extrinsicObject.MimeType = documentEntryMetadata.MimeType;
        }
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
                MiddleName = middleName,
                FamilyName = lastName,
                AssigningAuthority = new()
                {
                    UniversalId = legalAuthenticator.IdSystem,
                    UniversalIdType = legalAuthenticator.IdSystem == null ? null : Constants.Hl7.UniversalIdType.Iso
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
            extrinsicObject.Home = homeCommunityId;
        }
    }

    private void GetHealthCareFacilityTypeCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var healthcareFacilityTypeCode = documentEntryMetadata.HealthCareFacilityTypeCode;
        var healthcareFacilityTypeCodeClassification = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.HealthCareFacilityTypeCode, healthcareFacilityTypeCode);

        if (healthcareFacilityTypeCodeClassification != null)
        {
            healthcareFacilityTypeCodeClassification.ClassifiedObject = extrinsicObject.Id;
            extrinsicObject.Classification ??= [];
            extrinsicObject.Classification = [.. extrinsicObject.Classification, healthcareFacilityTypeCodeClassification];
        }
    }

    private void GetFormatCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var formatCode = documentEntryMetadata.FormatCode;
        var formatCodeClassification = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.FormatCode, formatCode);


        if (formatCodeClassification != null)
        {
            formatCodeClassification.ClassifiedObject = extrinsicObject.Id;
            extrinsicObject.Classification ??= [];
            extrinsicObject.Classification = [.. extrinsicObject.Classification, formatCodeClassification];
        }
    }

    private void GetEventCodeListClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var eventCode = documentEntryMetadata.EventCodeList;
        if (eventCode == null) return;

        var eventCodeClassification = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.EventCodeList, eventCode);

        eventCodeClassification.ClassifiedObject = extrinsicObject.Id;

        if (eventCodeClassification != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, eventCodeClassification];
        }
    }

    private void GetConfidentialityCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var confCode = documentEntryMetadata.ConfidentialityCode;
        if (confCode == null) return;

        var confCodeClassification = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode, confCode);


        if (confCodeClassification != null)
        {
            confCodeClassification.ClassifiedObject = extrinsicObject.Id;
            extrinsicObject.Classification ??= [];
            extrinsicObject.Classification = [.. extrinsicObject.Classification, confCodeClassification];
        }
    }

    private ClassificationType MapCodedValueToClassification(string healthCareFacilityTypeCode, CodedValue? confCode)
    {
        if (confCode == null) return null;

        return new()
        {
            ClassificationScheme = healthCareFacilityTypeCode,
            NodeRepresentation = confCode.Code,
            Name = confCode.DisplayName != null ? new() { LocalizedString = [new() { Value = confCode.DisplayName }] } : null,
            Slot = confCode.CodeSystem != null ? [new() { Name = Constants.Xds.SlotNames.CodingScheme, ValueList = new() { Value = [confCode.CodeSystem] } }] : null
        };

    }

    private void GetClassCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var classCode = documentEntryMetadata.ClassCode;

        if (classCode == null) return;

        var classCodeClassification = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.ClassCode, classCode);

        if (classCodeClassification != null)
        {
            classCodeClassification.ClassifiedObject = extrinsicObject.Id;
            extrinsicObject.Classification ??= [];
            extrinsicObject.Classification = [.. extrinsicObject.Classification, classCodeClassification];
        }
    }

    private void GetHashSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.Hash == null) return;

        extrinsicObject.AddSlot(Constants.Xds.SlotNames.Hash, [documentEntryMetadata.Hash]);
    }

    private void GetAuthorClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var authorClassification = new ClassificationType()
        {
            ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.Author,
            ClassifiedObject = extrinsicObject.Id
        };
        extrinsicObject.Classification ??= [];

        var author = documentEntryMetadata.Author;
        if (author != null)
        {
            GetAuthorPersonSlotFromAuthor(authorClassification, author);
            GetAuthorInstitutionSlotFromAuthor(authorClassification, author);
            GetAuthorRoleSlotFromAuthor(authorClassification, author);
            GetAuthorSpecialitySlotFromAuthor(authorClassification, author);
        }

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
                UniversalIdType = authorSpeciality.CodeSystem == null ? null : Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorSpecialty, [authorSpecialityCx.Serialize()]);
    }

    private void GetAuthorRoleSlotFromAuthor(ClassificationType classification, Author documentAuthor)
    {
        var authorRole = documentAuthor?.Role;
        if (authorRole == null) return;

        var authorRoleCx = new CX()
        {
            IdNumber = authorRole.Code,
            AssigningAuthority = new HD()
            {
                UniversalId = authorRole.CodeSystem,
                UniversalIdType = authorRole.CodeSystem == null ? null : Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorRole, [authorRoleCx.Serialize()]);
    }

    private void GetAuthorInstitutionSlotFromAuthor(ClassificationType classification, Author author)
    {
        if (author == null) return;

        var authorInstitutionSlot = new SlotType()
        {
            Name = Constants.Xds.SlotNames.AuthorInstitution,
            ValueList = new()
        };

        if (author.Organization != null)
        {
            var org = new XON()
            {
                OrganizationName = author.Organization.OrganizationName,
                OrganizationIdentifier = author.Organization.Id,
                AssigningAuthority = new()
                {
                    UniversalId = author.Organization.AssigningAuthority,
                    UniversalIdType = author.Organization.AssigningAuthority == null ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };

            authorInstitutionSlot.ValueList.Value = [org.Serialize()];
        }

        if (author.Department != null)
        {
            var dpt = new XON()
            {
                OrganizationName = author.Department.OrganizationName,
                OrganizationIdentifier = author.Department.Id,
                AssigningAuthority = new()
                {
                    UniversalId = author.Department.AssigningAuthority,
                    UniversalIdType = author.Department.AssigningAuthority == null ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };

            authorInstitutionSlot.ValueList.Value = [.. authorInstitutionSlot.ValueList.Value, dpt.Serialize()];
        }

        classification.AddSlot(authorInstitutionSlot);
    }

    private void GetAuthorPersonSlotFromAuthor(ClassificationType classification, Author author)
    {
        var authorPerson = author?.Person;
        if (authorPerson == null) return;

        var lastNameParts = authorPerson.LastName?.Split(' ');
        var middleName = lastNameParts.Length > 1 ? lastNameParts?.FirstOrDefault() : null;
        var lastName = lastNameParts.Length > 1 ? string.Join(" ", lastNameParts?.Skip(1)) : lastNameParts.FirstOrDefault();
        var authorXcn = new XCN()
        {
            PersonIdentifier = authorPerson.Id,
            GivenName = authorPerson.FirstName,
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
            FamilyName = lastName,
            AssigningAuthority = new()
            {
                UniversalId = authorPerson.AssigningAuthority,
                UniversalIdType = authorPerson.AssigningAuthority == null ? null : Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorPerson, [authorXcn.Serialize()]);
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
        if (!string.IsNullOrWhiteSpace(documentEntryMetadata?.ObjectType))
        {
            extrinsicObject.ObjectType = documentEntryMetadata.ObjectType;
        }
    }


    private void GetSourcePatientIdSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.PatientId?.Code != null)
        {
            var patientId = new CX()
            {
                IdNumber = documentEntryMetadata.PatientId.Code,
                AssigningAuthority = new()
                {
                    UniversalId = documentEntryMetadata.PatientId.CodeSystem,
                    UniversalIdType = documentEntryMetadata.PatientId.CodeSystem != null ? Constants.Hl7.UniversalIdType.Iso : null
                }
            };
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.SourcePatientId, [patientId.Serialize()]);
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
        var registryPackage = new RegistryPackageType()
        {
            Classification = [],
            ExternalIdentifier = []
        };
        registryPackage.Id = submissionSetMetadata.Id;
        GetAuthorClassificationFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetAvailabilitystatusFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetContentTypeCodeFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetHomeCommunityIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetPatientIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetSourceIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetSubmissionTimeSlotFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetTitleNameLocalizedStringFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetUniqueIdExternalIdentifierFromSubmissionSetDto(registryPackage, submissionSetMetadata);

        return registryPackage;
    }

    private void GetUniqueIdExternalIdentifierFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.UniqueId, submissionSetMetadata.UniqueId);
    }

    private void GetTitleNameLocalizedStringFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        if (!string.IsNullOrWhiteSpace(submissionSetMetadata.Title))
        {
            registryPackage.Name = new() { LocalizedString = [new() { Value = submissionSetMetadata.Title }] };
        }

    }

    private void GetSourceIdFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.SourceId, submissionSetMetadata.SourceId);

    }

    private void GetHomeCommunityIdFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var homeCommunityId = submissionSetMetadata.HomeCommunityId;
        if (homeCommunityId != null)
        {
            registryPackage.AddSlot(Constants.Xds.SlotNames.SubmissionTime, [homeCommunityId]);
        }
    }

    private void GetPatientIdFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var patientExternalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.PatientId, submissionSetMetadata.PatientId);

        registryPackage.ExternalIdentifier = [.. registryPackage.ExternalIdentifier, patientExternalIdentifier];
    }

    private ExternalIdentifierType MapCodedValueToExternalIdentifier(string? externalIdentifierName, string? codedValue)
    {
        if (!string.IsNullOrWhiteSpace(externalIdentifierName) && !string.IsNullOrWhiteSpace(codedValue))
        {
            return MapCodedValueToExternalIdentifier(externalIdentifierName, new CodedValue() { Code = codedValue });
        }
        return null;
    }

    private ExternalIdentifierType MapCodedValueToExternalIdentifier(string externalIdentifierName, CodedValue? codedValue)
    {
        if (codedValue == null || externalIdentifierName == null) return null;

        var idscheme = externalIdentifierName switch
        {
            Constants.Xds.ExternalIdentifierNames.SubmissionSetUniqueId => Constants.Xds.Uuids.SubmissionSet.UniqueId,
            Constants.Xds.ExternalIdentifierNames.SubmissionSetSourceId => Constants.Xds.Uuids.SubmissionSet.UniqueId,
            Constants.Xds.ExternalIdentifierNames.SubmissionSetPatientId => Constants.Xds.Uuids.SubmissionSet.UniqueId,
            Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId => Constants.Xds.Uuids.SubmissionSet.UniqueId,
            Constants.Xds.ExternalIdentifierNames.DocumentEntryPatientId => Constants.Xds.Uuids.SubmissionSet.UniqueId,
            _ => string.Empty
        };

        return new()
        {
            IdentificationScheme = idscheme,
            Value = codedValue.Code,
            Name = new() { LocalizedString = [new() { Value = externalIdentifierName }] },
        };

    }

    private void GetSubmissionTimeSlotFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var dateValue = submissionSetMetadata.SubmissionTime;
        if (dateValue != null)
        {
            registryPackage.AddSlot(Constants.Xds.SlotNames.SubmissionTime, [dateValue.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private void GetContentTypeCodeFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var contentTypeCode = submissionSetMetadata.ContentTypeCode;
        if (contentTypeCode == null) return;

        var contentTypeClassification = MapCodedValueToClassification(Constants.Xds.Uuids.SubmissionSet.ContentTypeCode, contentTypeCode);

        registryPackage.Classification = [.. registryPackage.Classification, contentTypeClassification];
    }

    private void GetAvailabilitystatusFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        if (submissionSetMetadata.AvailabilityStatus != null)
        {
            registryPackage.Status = submissionSetMetadata.AvailabilityStatus;
        }
    }

    private void GetAuthorClassificationFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var authorClassification = new ClassificationType();
        var submissionSetAuthor = submissionSetMetadata.Author;
        if (submissionSetAuthor == null) return;

        GetAuthorInstitutionSlotFromAuthor(authorClassification, submissionSetAuthor);
        GetAuthorPersonSlotFromAuthor(authorClassification, submissionSetAuthor);
        GetAuthorRoleSlotFromAuthor(authorClassification, submissionSetAuthor);
        GetAuthorSpecialitySlotFromAuthor(authorClassification, submissionSetAuthor);

        registryPackage.Classification = [.. registryPackage.Classification, authorClassification];
    }

    private AssociationType GetAssociationFromAssociationDto(AssociationDto association)
    {
        var ebRimAssociation = new AssociationType()
        {
            Id = association.Id,
            AssociationTypeData = association.AssociationType,
            SourceObject = association.SourceObject,
            TargetObject = association.TargetObject
        };
        ebRimAssociation.AddSlot(Constants.Xds.SlotNames.SubmissionSetStatus, [association.SubmissionSetStatus]);

        return ebRimAssociation;
    }
}
