﻿using System.Globalization;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

public static class RegistryMetadataTransformerService
{
    public static DocumentReferenceDto TransformRegistryObjectsToDocumentReferenceDto(ExtrinsicObjectType extrinsicObject, RegistryPackageType registryPackage, AssociationType association, DocumentType document = null)
    {
        var documentEntryDto = new DocumentReferenceDto();

        documentEntryDto.DocumentEntry = TransformExtrinsicObjectToDocumentEntryDto(extrinsicObject);
        documentEntryDto.SubmissionSet = TransformRegistryPackageToSubmissionSetDto(registryPackage);
        documentEntryDto.Association = TransformToAssociationDto(association, extrinsicObject, registryPackage);
        if (document?.Value != null)
        {
            documentEntryDto.Document = new() { DocumentId = document.Id, Data = document.Value };
        }

        return documentEntryDto;
    }

    public static List<IdentifiableType> TransformRegistryObjectDtosToRegistryObjects(List<DocumentEntryDto> registryObjectDtos)
    {
        return TransformRegistryObjectDtosToRegistryObjects(registryObjectDtos.Cast<RegistryObjectDto>().ToList());
    }

    public static List<IdentifiableType> TransformRegistryObjectDtosToRegistryObjects(List<SubmissionSetDto> registryObjectDtos)
    {
        return TransformRegistryObjectDtosToRegistryObjects(registryObjectDtos.Cast<RegistryObjectDto>().ToList());
    }

    public static List<IdentifiableType> TransformRegistryObjectDtosToRegistryObjects(List<AssociationDto> registryObjectDtos)
    {
        return TransformRegistryObjectDtosToRegistryObjects(registryObjectDtos.Cast<RegistryObjectDto>().ToList());
    }

    public static List<IdentifiableType> TransformRegistryObjectDtosToRegistryObjects(List<RegistryObjectDto> registryObjectDtos)
    {
        var registryObjects = new List<IdentifiableType>();

        if (registryObjectDtos == null || registryObjectDtos.Count == 0) return registryObjects;

        foreach (var registryObjectDto in registryObjectDtos)
        {
            if (registryObjectDto is AssociationDto associationDto)
            {
                var associationType = GetAssociationFromAssociationDto(associationDto);

                if (associationType == null) continue;

                registryObjects.Add(associationType);
                continue;
            }

            if (registryObjectDto is DocumentEntryDto documentEntryDto)
            {
                var extrinsicObjectType = GetExtrinsicObjectFromDocumentEntryDto(documentEntryDto);

                if (extrinsicObjectType == null) continue;

                registryObjects.Add(extrinsicObjectType);
                continue;
            }

            if (registryObjectDto is SubmissionSetDto submissionsetDto)
            {
                var associationType = GetRegistryPackageFromSubmissionSetDto(submissionsetDto);

                if (associationType == null) continue;

                registryObjects.Add(associationType);
                continue;
            }
        }

        return registryObjects;

    }

    public static List<DocumentEntryDto> TransformRegistryObjectsToRegistryObjectDtos(List<ExtrinsicObjectType> registryObjectList)
    {
        return TransformRegistryObjectsToRegistryObjectDtos(registryObjectList.Cast<IdentifiableType>().ToList()).Cast<DocumentEntryDto>().ToList();
    }

    public static List<SubmissionSetDto> TransformRegistryObjectsToRegistryObjectDtos(List<RegistryPackageType> registryObjectList)
    {
        return TransformRegistryObjectsToRegistryObjectDtos(registryObjectList.Cast<IdentifiableType>().ToList()).Cast<SubmissionSetDto>().ToList();
    }

    public static List<AssociationDto> TransformRegistryObjectsToRegistryObjectDtos(List<AssociationType> registryObjectList)
    {
        return TransformRegistryObjectsToRegistryObjectDtos(registryObjectList.Cast<IdentifiableType>().ToList()).Cast<AssociationDto>().ToList();
    }

    public static List<RegistryObjectDto> TransformRegistryObjectsToRegistryObjectDtos(List<IdentifiableType> registryObjectList)
    {
        var listDto = new List<RegistryObjectDto>();

        foreach (var registryObject in registryObjectList)
        {
            if (registryObject == null) continue;

            if (registryObject is AssociationType association)
            {
                var associationDto = TransformAssociationToAssociationDto(association);

                if (associationDto == null) continue;

                listDto.Add(associationDto);
                continue;
            }

            if (registryObject is ExtrinsicObjectType extrinsicObject)
            {
                var documentEntryDto = TransformExtrinsicObjectToDocumentEntryDto(extrinsicObject);
                
                if (documentEntryDto == null) continue;

                listDto.Add(documentEntryDto);
                continue;
            }

            if (registryObject is RegistryPackageType registryPackage)
            {
                var submissionSetDto = TransformRegistryPackageToSubmissionSetDto(registryPackage);
                
                if (submissionSetDto == null) continue;

                listDto.Add(submissionSetDto);
                continue;
            }
        }

        return listDto;
    }


    private static AssociationDto? TransformAssociationToAssociationDto(AssociationType association)
    {
        return TransformToAssociationDto(association, null, null);
    }

    private static AssociationDto? TransformToAssociationDto(AssociationType association, ExtrinsicObjectType? extrinsicObject = null, RegistryPackageType? registryPackage = null)
    {
        if (association == null) return null;

        var associationDto = new AssociationDto();

        associationDto.Id = association.Id;
        associationDto.SourceObject = association?.SourceObject ?? registryPackage?.Id;
        associationDto.TargetObject = association?.TargetObject ?? extrinsicObject?.Id;
        associationDto.AssociationType = association?.AssociationTypeData ?? Constants.Xds.AssociationType.HasMember;
        associationDto.SubmissionSetStatus = association?.GetFirstSlot(Constants.Xds.SlotNames.SubmissionSetStatus)?.GetFirstValue() ?? "Original";
        return associationDto;
    }

    private static SubmissionSetDto? TransformRegistryPackageToSubmissionSetDto(RegistryPackageType? registryPackage)
    {
        if (registryPackage == null) return null;

        var submissionSetDto = new SubmissionSetDto();

        submissionSetDto.Author = GetAuthorFromRegistryPackage(registryPackage);
        submissionSetDto.AvailabilityStatus = registryPackage.Status;
        submissionSetDto.HomeCommunityId = registryPackage.Home;
        submissionSetDto.Id = registryPackage.Id;
        submissionSetDto.PatientId = GetPatientIdFromRegistryPackage(registryPackage);
        submissionSetDto.SourceId = GetSourceIdFromRegistryPackage(registryPackage);
        submissionSetDto.SubmissionTime = GetSubmissionTimeFromRegistryPackage(registryPackage);
        submissionSetDto.Title = GetTitleFromRegistryPackage(registryPackage);
        submissionSetDto.UniqueId = GetUniqueIdFromRegistryPackage(registryPackage);


        return submissionSetDto;
    }

    private static string? GetUniqueIdFromRegistryPackage(RegistryPackageType? registryPackage)
    {
        return registryPackage?.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.UniqueId)?.Value;
    }

    private static string? GetTitleFromRegistryPackage(RegistryPackageType? registryPackage)
    {
        return registryPackage?.Name.GetFirstValue();
    }

    private static DateTime? GetSubmissionTimeFromRegistryPackage(RegistryPackageType? registryPackage)
    {
        var dateValue = registryPackage?.GetFirstSlot(Constants.Xds.SlotNames.SubmissionTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private static string? GetSourceIdFromRegistryPackage(RegistryPackageType registryPackage)
    {
        return registryPackage.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.SourceId)?.Value;
    }

    private static CodedValue? GetPatientIdFromRegistryPackage(RegistryPackageType? registryPackage)
    {
        var patientIdExtIder = registryPackage?.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.PatientId)?.Value;
        var patientIdValue = Hl7Object.Parse<CX>(patientIdExtIder);

        if (patientIdValue?.IdNumber != null)
        {
            return new()
            {
                Code = patientIdValue?.IdNumber,
                CodeSystem = patientIdValue?.AssigningAuthority?.UniversalId
            };
        }

        return null;
    }

    private static AuthorInfo? GetAuthorFromRegistryPackage(RegistryPackageType? registryPackage)
    {
        var authorClassification = registryPackage?.GetFirstClassification(Constants.Xds.Uuids.SubmissionSet.Author);

        if (authorClassification != null)
        {
            var author = new AuthorInfo();
            author.Organization = GetAuthorOrganizationFromClassification(authorClassification);
            author.Department = GetAuthorDepartmentFromClassification(authorClassification);
            author.Person = GetAuthorPersonFromClassification(authorClassification);
            author.Role = GetAuthorRoleFromClassificaiton(authorClassification);
            author.Speciality = GetAuthorSpecialityFromClassification(authorClassification);

            return author;
        }

        return null;
    }

    private static DocumentEntryDto TransformExtrinsicObjectToDocumentEntryDto(ExtrinsicObjectType extrinsicObject)
    {
        if (extrinsicObject == null) return null;

        var documentMetadata = new DocumentEntryDto();

        documentMetadata.Author = GetAuthorFromExtrinsicObject(extrinsicObject);
        documentMetadata.AvailabilityStatus = extrinsicObject.Status;
        documentMetadata.ClassCode = GetClassCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.ConfidentialityCode = GetConfidentialityCodeFromExtrinsicObject(extrinsicObject);
        documentMetadata.CreationTime = GetCreationTimeFromExtrinsicObject(extrinsicObject);
        documentMetadata.UniqueId = GetDocumentUniqueIdFromExtrinsicObject(extrinsicObject);
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
        documentMetadata.PatientId = GetPatientIdFromExtrinsicObject(extrinsicObject);
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

    private static CodedValue? GetTypeCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var typeCode = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.TypeCode);

        if (typeCode != null)
        {
            return MapClassificationToCodedValue(typeCode);
        }
        return null;
    }

    private static string? GetTitleFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.Name.GetFirstValue();
    }

    private static SourcePatientInfo? GetSourcePatientInfoFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var sourcePatientInfo = extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientInfo)?.GetValues();
        var sourcePatientId = extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientId)?.GetFirstValue();

        if (sourcePatientInfo != null)
        {
            var srcPatientId = Hl7Object.Parse<CX>(sourcePatientId);
            var patientId = Hl7Object.Parse<CX>(sourcePatientInfo.FirstOrDefault(s => s.Contains("PID-3"))?.Split("PID-3|")?.LastOrDefault());
            var name = Hl7Object.Parse<XPN>(sourcePatientInfo.FirstOrDefault(s => s.Contains("PID-5"))?.Split("PID-5|")?.LastOrDefault());
            var birthTime = sourcePatientInfo.FirstOrDefault(s => s.Contains("PID-7"))?.Split("PID-7|").LastOrDefault();
            var gender = sourcePatientInfo.FirstOrDefault(s => s.Contains("PID-8"))?.Split("PID-8|").LastOrDefault();


            return new()
            {
                BirthTime = birthTime == null ? null : DateTime.ParseExact(birthTime, Constants.Hl7.Dtm.DtmYmdFormat, CultureInfo.InvariantCulture),
                LastName = name?.FamilyName,
                FirstName = name?.GivenName,
                Gender = gender ?? "U",
                PatientId = new()
                {
                    Id = srcPatientId?.IdNumber ?? patientId?.IdNumber,
                    System = srcPatientId?.AssigningAuthority?.UniversalId ?? srcPatientId?.AssigningFacility?.UniversalId ?? patientId?.AssigningFacility?.UniversalId ?? patientId?.AssigningAuthority?.UniversalId,
                }
            };
        }

        return null;
    }

    private static string? GetSizeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        return extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.Size)?.GetFirstValue();
    }

    private static DateTime? GetServiceStopTimeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var dateValue = extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.ServiceStopTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.AllFormats, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private static DateTime? GetServiceStartTimeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var dateValue = extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.ServiceStartTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.AllFormats, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private static string? GetRepositoryUniqueIdFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        return extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.RepositoryUniqueId)?.GetFirstValue();
    }

    private static CodedValue? GetPracticeSettingCodeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var practiceSettingClassification = extrinsicObject?.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode);

        if (practiceSettingClassification != null)
        {
            return MapClassificationToCodedValue(practiceSettingClassification);
        }

        return null;
    }

    private static CodedValue? GetPatientIdFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var patientIdExtIder = extrinsicObject?.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.PatientId)?.Value;
        var patIdPid = Hl7Object.Parse<CX>(patientIdExtIder);

        if (patIdPid != null)
        {
            return new()
            {
                Code = patIdPid?.IdNumber,
                CodeSystem = patIdPid?.AssigningAuthority?.UniversalId
            };
        }

        return null;
    }

    private static LegalAuthenticator? GetLegalAuthenticatorFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var legalAuthenticator = new LegalAuthenticator();

        var legalAuthenticatorSlot = extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.LegalAuthenticator)?.GetFirstValue();

        var legAuthXcn = Hl7Object.Parse<XCN>(legalAuthenticatorSlot);

        if (legAuthXcn != null)
        {
            legalAuthenticator.LastName = legAuthXcn.FamilyName;
            legalAuthenticator.FirstName = legAuthXcn.GivenName;
            legalAuthenticator.Id = legAuthXcn.PersonIdentifier;
            legalAuthenticator.IdSystem = legAuthXcn.AssigningAuthority.UniversalId;

            return legalAuthenticator;
        }

        return null;



    }

    private static string? GetLanguageCodeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        return extrinsicObject?.GetFirstSlot(Constants.Xds.SlotNames.LanguageCode)?.GetFirstValue();
    }

    private static string? GetHashFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        return extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.Hash)?.GetFirstValue();
    }

    private static CodedValue? GetHealthCareFacilityTypeCodeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var healthcareTypeCodeClassificaiton = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.HealthCareFacilityTypeCode);

        if (healthcareTypeCodeClassificaiton != null)
        {
            return MapClassificationToCodedValue(healthcareTypeCodeClassificaiton);
        }

        return null;
    }

    private static CodedValue? GetFormatCodeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var formatCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.FormatCode);

        if (formatCodeClassification != null)
        {
            return MapClassificationToCodedValue(formatCodeClassification);
        }

        return null;
    }

    private static CodedValue? GetEventCodeListFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var eventCodeListClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.EventCodeList);

        if (eventCodeListClassification != null)
        {
            return MapClassificationToCodedValue(eventCodeListClassification);
        }

        return null;
    }

    private static string? GetDocumentUniqueIdFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var documentUniqueId = extrinsicObject.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value;
        return documentUniqueId;
    }

    private static DateTime? GetCreationTimeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var dateValue = extrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.CreationTime)?.GetFirstValue();
        if (dateValue != null)
        {
            return DateTime.ParseExact(dateValue, Constants.Hl7.Dtm.AllFormats, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private static CodedValue? GetConfidentialityCodeFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {

        var confCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode);

        if (confCodeClassification != null)
        {
            return MapClassificationToCodedValue(confCodeClassification);
        }

        return null;
    }

    private static CodedValue? GetClassCodeFromExtrinsicObject(ExtrinsicObjectType extrinsicObject)
    {
        var classCodeClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ClassCode);

        if (classCodeClassification != null)
        {
            return MapClassificationToCodedValue(classCodeClassification);
        }

        return null;
    }

    public static CodedValue? MapClassificationToCodedValue(ClassificationType? classification)
    {
        if (classification == null) return null;

        var classNodeRep = classification?.NodeRepresentation;
        var classValue = classification?.GetFirstSlot()?.GetFirstValue();
        var className = classification?.Name?.GetFirstValue();

        if (classNodeRep == null && classValue == null && className == null) return null;

        return new()
        {
            Code = classNodeRep,
            CodeSystem = classValue,
            DisplayName = className
        };
    }

    private static AuthorInfo GetAuthorFromExtrinsicObject(ExtrinsicObjectType? extrinsicObject)
    {
        var authorClassification = extrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.Author);

        if (authorClassification != null)
        {
            var author = new AuthorInfo();
            author.Organization = GetAuthorOrganizationFromClassification(authorClassification);
            author.Department = GetAuthorDepartmentFromClassification(authorClassification);
            author.Person = GetAuthorPersonFromClassification(authorClassification);
            author.Role = GetAuthorRoleFromClassificaiton(authorClassification);
            author.Speciality = GetAuthorSpecialityFromClassification(authorClassification);
            return author;
        }
        return null;

    }
    private static AuthorOrganization? GetAuthorDepartmentFromClassification(ClassificationType authorClassification)
    {
        if (authorClassification == null) return null;

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
                AssigningAuthority = department?.AssigningAuthority?.UniversalId ?? department?.AssigningFacility?.UniversalId,
                Id = department?.OrganizationIdentifier,
                OrganizationName = department?.OrganizationName
            };
        }

        return null;
    }

    private static AuthorOrganization? GetAuthorOrganizationFromClassification(ClassificationType authorClassification)
    {
        if (authorClassification == null) return null;

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
            authorOrganization.AssigningAuthority = organization?.AssigningAuthority?.UniversalId ?? organization?.AssigningFacility?.UniversalId;
            authorOrganization.Id = organization?.OrganizationIdentifier;
            authorOrganization.OrganizationName = organization?.OrganizationName;
            return authorOrganization;
        }

        return null;
    }

    private static AuthorPerson? GetAuthorPersonFromClassification(ClassificationType authorClassification)
    {
        if (authorClassification == null) return null;

        var authorPerson = new AuthorPerson();

        var authorPersonXcn = authorClassification
            .GetSlots(Constants.Xds.SlotNames.AuthorPerson)
            .GetValues()
            .Select(asl => Hl7Object.Parse<XCN>(asl)).FirstOrDefault();

        // Resolve whether authorPerson is stored as a simple string or as XCN
        if (authorPersonXcn?.PersonIdentifier != null && authorPersonXcn?.GivenName == null)
        {
            var nameParts = authorPersonXcn?.PersonIdentifier.Split(' ');
            authorPerson.FirstName = nameParts?.FirstOrDefault();
            authorPerson.LastName = string.Join(' ', nameParts?.Skip(1));
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

    private static CodedValue? GetAuthorRoleFromClassificaiton(ClassificationType classification)
    {
        if (classification == null) return null;

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

    private static CodedValue? GetAuthorSpecialityFromClassification(ClassificationType authorClassification)
    {
        if (authorClassification == null) return null;

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

    public static IdentifiableType[] TransformDocumentReferenceDtoListToRegistryObjects(List<RegistryObjectDto> documentReferences)
    {
        var registryObjectList = new List<IdentifiableType>();
        registryObjectList.AddRange(TransformRegistryObjectDtosToRegistryObjects(documentReferences));

        return registryObjectList.ToArray();
    }


    public static IdentifiableType[] TransformDocumentReferenceDtoToRegistryObjects(DocumentReferenceDto documentReference)
    {
        var registryObjectList = new List<IdentifiableType>();

        var extrinsicObject = GetExtrinsicObjectFromDocumentEntryDto(documentReference.DocumentEntry);
        var registryPackage = GetRegistryPackageFromSubmissionSetDto(documentReference.SubmissionSet);
        var association = GetAssociationFromAssociationDto(documentReference.Association);

        registryObjectList.Add(extrinsicObject);
        registryObjectList.Add(registryPackage);
        registryObjectList.Add(association);

        return registryObjectList.ToArray();
    }

    private static ExtrinsicObjectType GetExtrinsicObjectFromDocumentEntryDto(DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata == null) return null;

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
        GetObjectTypeFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
        GetPatientIdExternalIdentifierFromDocumentEntryDto(documentEntryMetadata, extrinsicObject);
        GetPracticeSettingCodeFromDocumentEntryDto(extrinsicObject, documentEntryMetadata);
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

    private static RegistryPackageType GetRegistryPackageFromSubmissionSetDto(SubmissionSetDto submissionSetMetadata)
    {
        var registryPackage = new RegistryPackageType()
        {
            ObjectType = Constants.Xds.ObjectTypes.RegistryPackage,
            Classification = [],
            ExternalIdentifier = []
        };

        registryPackage.Id = submissionSetMetadata.Id;
        GetAuthorClassificationFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetAvailabilitystatusFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetHomeCommunityIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetPatientIdFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetSourceIdExternalIdentifierFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetSubmissionTimeSlotFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetTitleNameLocalizedStringFromSubmissionSetDto(registryPackage, submissionSetMetadata);
        GetUniqueIdExternalIdentifierFromSubmissionSetDto(registryPackage, submissionSetMetadata);

        return registryPackage;
    }

    private static AssociationType GetAssociationFromAssociationDto(AssociationDto association)
    {
        var ebRimAssociation = new AssociationType()
        {
            ObjectType = Constants.Xds.ObjectTypes.Association,
            Id = association.Id,
            AssociationTypeData = association.AssociationType,
            SourceObject = association.SourceObject,
            TargetObject = association.TargetObject
        };
        ebRimAssociation.AddSlot(Constants.Xds.SlotNames.SubmissionSetStatus, [association.SubmissionSetStatus]);

        return ebRimAssociation;
    }

    private static void GetUniqueIdExternalIdentifierFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId, documentEntryMetadata.UniqueId);
        if (externalIdentifier != null)
        {
            externalIdentifier.RegistryObject = extrinsicObject.Id;
            extrinsicObject.ExternalIdentifier = [.. extrinsicObject.ExternalIdentifier, externalIdentifier];
        }
    }

    private static void GetTypeCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var typeCodeClass = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.TypeCode, documentEntryMetadata.TypeCode);
        if (typeCodeClass != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, typeCodeClass];
        }
    }

    private static void GetPracticeSettingCodeFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var practiceSettingCode = MapCodedValueToClassification(Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode, documentEntryMetadata.PracticeSettingCode);
        if (practiceSettingCode != null)
        {
            extrinsicObject.Classification = [.. extrinsicObject.Classification, practiceSettingCode];
        }
    }

    private static void GetPatientIdExternalIdentifierFromDocumentEntryDto(DocumentEntryDto documentEntryMetadata, ExtrinsicObjectType extrinsicObject)
    {
        extrinsicObject.ExternalIdentifier ??= [];

        var patientIdExtId = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.PatientId, documentEntryMetadata.PatientId);
        if (patientIdExtId != null)
        {
            patientIdExtId.RegistryObject = extrinsicObject.Id;
            extrinsicObject.ExternalIdentifier = [.. extrinsicObject.ExternalIdentifier, patientIdExtId];
        }
    }

    private static void GetSizeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.Size != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.Size, [documentEntryMetadata.Size]);
        }
    }

    private static void GetRepositoryUniqueIdSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var repositoryUniqueId = documentEntryMetadata.RepositoryUniqueId;
        if (repositoryUniqueId != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.RepositoryUniqueId, [repositoryUniqueId]);
        }
    }

    private static void GetSourcePatientInfoSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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
                    UniversalIdType = string.IsNullOrWhiteSpace(sourcePatientInfo?.PatientId?.System) ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };

            sourcePatientIdSlot.AddValue($"PID-3|{patientId.Serialize()}");
        }


        if (sourcePatientInfo?.LastName != null)
        {
            var lastNameParts = sourcePatientInfo.LastName?.Split(' ');

            var sourcePatientXcn = new XPN()
            {
                GivenName = sourcePatientInfo.FirstName,
                FamilyName = sourcePatientInfo.LastName,
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

    private static void GetServiceStopTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var serviceStartTime = documentEntryMetadata.ServiceStopTime;
        if (serviceStartTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.ServiceStopTime, [serviceStartTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private static void GetServiceStartTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var serviceStartTime = documentEntryMetadata.ServiceStartTime;
        if (serviceStartTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.ServiceStartTime, [serviceStartTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private static void GetTitleNameLocalizedStringFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.Title != null)
        {
            extrinsicObject.Name = new() { LocalizedString = [new() { Value = documentEntryMetadata.Title }] };
        }
    }

    private static void GetMimeTypeFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (!string.IsNullOrWhiteSpace(documentEntryMetadata.MimeType))
        {
            extrinsicObject.MimeType = documentEntryMetadata.MimeType;
        }
    }

    private static void GetLegalAuthenticatorSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var legalAuthenticator = documentEntryMetadata.LegalAuthenticator;
        if (legalAuthenticator != null)
        {
            string middleName = null;
            string lastName = null;

            var lastNameParts = legalAuthenticator.LastName?.Split(' ');

            if (lastNameParts != null)
            {
                middleName = lastNameParts?.FirstOrDefault();
                lastName = string.Join(" ", lastNameParts?.Skip(1));
            }

            var legalAuthXcn = new XCN()
            {
                PersonIdentifier = legalAuthenticator.Id,
                GivenName = legalAuthenticator.FirstName,
                MiddleName = middleName,
                FamilyName = lastName,
                AssigningAuthority = new()
                {
                    UniversalId = legalAuthenticator.IdSystem,
                    UniversalIdType = string.IsNullOrWhiteSpace(legalAuthenticator.IdSystem) ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };

            extrinsicObject.AddSlot(Constants.Xds.SlotNames.LegalAuthenticator, [legalAuthXcn.Serialize()]);
        }
    }

    private static void GetHomeCommunityIdFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        var homeCommunityId = documentEntryMetadata.HomeCommunityId;
        if (homeCommunityId != null)
        {
            extrinsicObject.Home = homeCommunityId;
        }
    }

    private static void GetHealthCareFacilityTypeCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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

    private static void GetFormatCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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

    private static void GetEventCodeListClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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

    private static void GetConfidentialityCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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

    private static void GetClassCodeClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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

    private static void GetHashSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.Hash == null) return;

        extrinsicObject.AddSlot(Constants.Xds.SlotNames.Hash, [documentEntryMetadata.Hash]);
    }

    private static void GetAuthorClassificationFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
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

    private static void GetAuthorSpecialitySlotFromAuthor(ClassificationType classification, AuthorInfo documentAuthor)
    {
        var authorSpeciality = documentAuthor?.Speciality;

        if (authorSpeciality == null) return;

        var authorSpecialityCx = new CX()
        {
            IdNumber = authorSpeciality.Code,
            AssigningAuthority = new HD()
            {
                UniversalId = authorSpeciality.CodeSystem,
                UniversalIdType = string.IsNullOrWhiteSpace(authorSpeciality.CodeSystem) ? null : Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorSpecialty, [authorSpecialityCx.Serialize()]);
    }

    private static void GetAuthorRoleSlotFromAuthor(ClassificationType classification, AuthorInfo documentAuthor)
    {
        var authorRole = documentAuthor?.Role;
        if (authorRole == null) return;

        var authorRoleCx = new CX()
        {
            IdNumber = authorRole.Code,
            AssigningAuthority = new HD()
            {
                UniversalId = authorRole.CodeSystem,
                UniversalIdType = string.IsNullOrWhiteSpace(authorRole.CodeSystem) ? null : Constants.Hl7.UniversalIdType.Iso
            }
        };

        classification.AddSlot(Constants.Xds.SlotNames.AuthorRole, [authorRoleCx.Serialize()]);
    }

    private static void GetAuthorInstitutionSlotFromAuthor(ClassificationType classification, AuthorInfo author)
    {
        if (author == null) return;

        var authorInstitutionSlot = new SlotType()
        {
            Name = Constants.Xds.SlotNames.AuthorInstitution,
            ValueList = new()
        };

        var organization = new XON();
        var department = new XON();

        if (author.Organization != null)
        {
            var org = new XON()
            {
                OrganizationName = author.Organization.OrganizationName,
                OrganizationIdentifier = author.Organization.Id,
                AssigningAuthority = new()
                {
                    UniversalId = author.Organization.AssigningAuthority,
                    UniversalIdType = string.IsNullOrWhiteSpace(author.Organization.AssigningAuthority) ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };
            organization = org;
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
                    UniversalIdType = string.IsNullOrWhiteSpace(author.Department.AssigningAuthority) ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };
            department = dpt;

        }

        authorInstitutionSlot.ValueList.Value = [organization.Serialize(), department.Serialize()];

        classification.AddSlot(authorInstitutionSlot);
    }

    private static void GetAuthorPersonSlotFromAuthor(ClassificationType classification, AuthorInfo author)
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
                UniversalIdType = string.IsNullOrWhiteSpace(authorPerson.AssigningAuthority) ? null : Constants.Hl7.UniversalIdType.Iso
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

    private static void GetObjectTypeFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (!string.IsNullOrWhiteSpace(documentEntryMetadata?.ObjectType))
        {
            extrinsicObject.ObjectType = documentEntryMetadata.ObjectType;
        }
    }

    private static void GetSourcePatientIdSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.SourcePatientInfo?.PatientId != null)
        {
            var patientId = new CX()
            {
                IdNumber = documentEntryMetadata.SourcePatientInfo.PatientId.Id,
                AssigningAuthority = new()
                {
                    UniversalId = documentEntryMetadata.SourcePatientInfo.PatientId.System,
                    UniversalIdType = string.IsNullOrWhiteSpace(documentEntryMetadata.SourcePatientInfo.PatientId.System) ? null : Constants.Hl7.UniversalIdType.Iso
                }
            };
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.SourcePatientId, [patientId.Serialize()]);
        }
    }

    private static void GetLanguageCodeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata.LanguageCode != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.LanguageCode, [documentEntryMetadata.LanguageCode]);
        }
    }

    private static void GetCreationTimeSlotFromDocumentEntryDto(ExtrinsicObjectType extrinsicObject, DocumentEntryDto documentEntryMetadata)
    {
        if (documentEntryMetadata?.CreationTime != null)
        {
            extrinsicObject.AddSlot(Constants.Xds.SlotNames.CreationTime, [documentEntryMetadata.CreationTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private static void GetUniqueIdExternalIdentifierFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.UniqueId, submissionSetMetadata.UniqueId);
        if (externalIdentifier != null)
        {
            externalIdentifier.RegistryObject = registryPackage.Id;
            registryPackage.ExternalIdentifier = [.. registryPackage.ExternalIdentifier, externalIdentifier];
        }
    }

    private static void GetTitleNameLocalizedStringFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        if (!string.IsNullOrWhiteSpace(submissionSetMetadata.Title))
        {
            registryPackage.Name = new() { LocalizedString = [new() { Value = submissionSetMetadata.Title }] };
        }

    }

    private static void GetSourceIdExternalIdentifierFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.SourceId, submissionSetMetadata.SourceId);
        if (externalIdentifier != null)
        {
            externalIdentifier.RegistryObject = registryPackage.Id;
            registryPackage.ExternalIdentifier = [.. registryPackage.ExternalIdentifier, externalIdentifier];
        }
    }

    private static void GetHomeCommunityIdFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var homeCommunityId = submissionSetMetadata.HomeCommunityId;
        if (homeCommunityId != null)
        {
            registryPackage.AddSlot(Constants.Xds.SlotNames.SubmissionTime, [homeCommunityId]);
        }
    }

    private static void GetPatientIdFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var externalIdentifier = MapCodedValueToExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.PatientId, submissionSetMetadata.PatientId);

        if (externalIdentifier != null)
        {
            externalIdentifier.RegistryObject = registryPackage.Id;
            registryPackage.ExternalIdentifier = [.. registryPackage.ExternalIdentifier, externalIdentifier];
        }
    }

    private static void GetSubmissionTimeSlotFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        var dateValue = submissionSetMetadata.SubmissionTime;
        if (dateValue != null)
        {
            registryPackage.AddSlot(Constants.Xds.SlotNames.SubmissionTime, [dateValue.Value.ToString(Constants.Hl7.Dtm.DtmFormat)]);
        }
    }

    private static void GetAvailabilitystatusFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
    {
        if (submissionSetMetadata.AvailabilityStatus != null)
        {
            registryPackage.Status = submissionSetMetadata.AvailabilityStatus;
        }
    }

    private static void GetAuthorClassificationFromSubmissionSetDto(RegistryPackageType registryPackage, SubmissionSetDto submissionSetMetadata)
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

    private static ExternalIdentifierType MapCodedValueToExternalIdentifier(string? externalIdentifierName, string? codedValue)
    {
        if (!string.IsNullOrWhiteSpace(externalIdentifierName) && !string.IsNullOrWhiteSpace(codedValue))
        {
            return MapCodedValueToExternalIdentifier(externalIdentifierName, new CodedValue() { Code = codedValue });
        }
        return null;
    }

    private static ExternalIdentifierType MapCodedValueToExternalIdentifier(string externalIdentifierName, CodedValue? codedValue)
    {
        if (codedValue == null || externalIdentifierName == null) return null;

        var idscheme = externalIdentifierName switch
        {
            Constants.Xds.Uuids.SubmissionSet.UniqueId => Constants.Xds.ExternalIdentifierNames.SubmissionSetUniqueId,
            Constants.Xds.Uuids.SubmissionSet.SourceId => Constants.Xds.ExternalIdentifierNames.SubmissionSetSourceId,
            Constants.Xds.Uuids.SubmissionSet.PatientId => Constants.Xds.ExternalIdentifierNames.SubmissionSetPatientId,
            Constants.Xds.Uuids.DocumentEntry.UniqueId => Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId,
            Constants.Xds.Uuids.DocumentEntry.PatientId => Constants.Xds.ExternalIdentifierNames.DocumentEntryPatientId,
            _ => string.Empty
        };

        var valueCx = new CX()
        {
            IdNumber = codedValue.Code,
            AssigningAuthority = new()
            {
                UniversalId = codedValue.CodeSystem,
                UniversalIdType = string.IsNullOrWhiteSpace(codedValue.CodeSystem) ? null : Constants.Hl7.UniversalIdType.Iso
            }
        };

        return new()
        {
            ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
            IdentificationScheme = externalIdentifierName,
            Value = valueCx.Serialize(),
            Name = new() { LocalizedString = [new() { Value = idscheme }] },
        };
    }

    private static ClassificationType MapCodedValueToClassification(string healthCareFacilityTypeCode, CodedValue? confCode)
    {
        if (confCode == null) return null;

        return new()
        {
            ClassificationScheme = healthCareFacilityTypeCode,
            NodeRepresentation = confCode?.Code,
            Name = confCode?.DisplayName != null ? new() { LocalizedString = [new() { Value = confCode.DisplayName }] } : null,
            Slot = confCode?.CodeSystem != null ? [new() { Name = Constants.Xds.SlotNames.CodingScheme, ValueList = new() { Value = [confCode.CodeSystem] } }] : null
        };

    }
}
