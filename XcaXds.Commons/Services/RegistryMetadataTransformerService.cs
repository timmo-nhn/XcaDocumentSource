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

        if (department != null && department?.IdNumber != organization?.IdNumber)
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
            authorOrganization.Id = organization?.IdNumber ?? string.Empty;
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

        var extrinsicObject = MapFromDocumentReferenceToExtrinsicObject(documentReference.DocumentEntryMetadata);
        var registryPackage = MapFromSubmissionSetDtoToRegistryPackage(documentReference.SubmissionSetMetadata);
        var association = MapFromAssociationDtoToAssociation(documentReference.Association);

        registryObjectList.Add(extrinsicObject);

        return registryObjectList.ToArray();
    }

    private ExtrinsicObjectType MapFromDocumentReferenceToExtrinsicObject(DocumentEntryDto documentEntryMetadata)
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

    private void GetAuthorClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var classification = new ClassificationType();

        GetAuthorPersonSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetAuthorInstitutionSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetAuthorRoleSlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetAuthorSpecialitySlotFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);

        
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

    private RegistryPackageType MapFromSubmissionSetDtoToRegistryPackage(SubmissionSetDto submissionSetMetadata)
    {
        throw new NotImplementedException();
    }

    private AssociationType MapFromAssociationDtoToAssociation(AssociationDto association)
    {
        throw new NotImplementedException();
    }
}
