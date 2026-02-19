using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Models.DatabaseDtos;
using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Source;

public static class DatabaseMapper
{
    public static RegistryObjectDto? MapFromDatabaseEntityToDto(DbRegistryObject registryObjects)
    {
        return MapFromDatabaseEntityToDto([registryObjects]).FirstOrDefault();
    }

    public static List<RegistryObjectDto> MapFromDatabaseEntityToDto(List<DbRegistryObject> registryObjects)
    {
        var registryObjectDtos = new List<RegistryObjectDto>();
        if (registryObjects?.Count == 0) return registryObjectDtos;

        foreach (var documentEntry in registryObjects?.OfType<DbDocumentEntry>() ?? [])
        {
            registryObjectDtos.Add(new DocumentEntryDto()
            {
                Author = documentEntry.Author?.Select(a => new AuthorInfo()
                {
                    Organization = new()
                    {
                        Id = a.OrganizationId,
                        OrganizationName = a.OrganizationName,
                        AssigningAuthority = a.OrganizationAssigningAuthority
                    },
                    Department = new()
                    {
                        Id = a.DepartmentId,
                        OrganizationName = a.DepartmentName,
                        AssigningAuthority = a.DepartmentAssigningAuthority
                    },
                    Person = new()
                    {
                        Id = a.PersonId,
                        AssigningAuthority = a.PersonAssigningAuthority,
                        FirstName = a.PersonFirstName,
                        LastName = a.PersonLastName
                    },
                    Role = new()
                    {
                        Code = a.RoleCode,
                        CodeSystem = a.RoleCodeSystem,
                        DisplayName = a.RoleDisplayName
                    },
                    Speciality = new()
                    {
                        Code = a.SpecialityCode,
                        CodeSystem = a.SpecialityCodeSystem,
                        DisplayName = a.SpecialityDisplayName
                    }
                }).ToList(),

                AvailabilityStatus = documentEntry.AvailabilityStatus,
                ClassCode = new()
                {
                    Code = documentEntry.ClassCode?.Code,
                    CodeSystem = documentEntry.ClassCode?.CodeSystem,
                    DisplayName = documentEntry.ClassCode?.DisplayName
                },
                ConfidentialityCode = documentEntry.ConfidentialityCode?.Select(c => new CodedValue()
                {
                    Code = c.Code,
                    CodeSystem = c.CodeSystem,
                    DisplayName = c.DisplayName
                }).ToList(),

                CreationTime = documentEntry.CreationTime,
                EventCodeList = new()
                {
                    Code = documentEntry.EventCodeList?.Code,
                    CodeSystem = documentEntry.EventCodeList?.CodeSystem,
                    DisplayName = documentEntry.EventCodeList?.DisplayName
                },
                FormatCode = new()
                {
                    Code = documentEntry.FormatCode?.Code,
                    CodeSystem = documentEntry.FormatCode?.CodeSystem,
                    DisplayName = documentEntry.FormatCode?.DisplayName,
                },
                Hash = documentEntry.Hash,
                HealthCareFacilityTypeCode = new()
                {
                    Code = documentEntry.HealthCareFacilityTypeCode?.Code,
                    CodeSystem = documentEntry.HealthCareFacilityTypeCode?.CodeSystem,
                    DisplayName = documentEntry.HealthCareFacilityTypeCode?.DisplayName,
                },
                HomeCommunityId = documentEntry.HomeCommunityId,
                LanguageCode = documentEntry.LanguageCode,
                LegalAuthenticator = new()
                {
                    Id = documentEntry.LegalAuthenticator?.Id,
                    IdSystem = documentEntry.LegalAuthenticator?.IdSystem,
                    FirstName = documentEntry.LegalAuthenticator?.FirstName,
                    LastName = documentEntry.LegalAuthenticator?.LastName,
                },
                Id = documentEntry.Id,
                MimeType = documentEntry.MimeType,
                ObjectType = documentEntry.ObjectType,
                PracticeSettingCode = new()
                {
                    Code = documentEntry.PracticeSettingCode?.Code,
                    CodeSystem = documentEntry.PracticeSettingCode?.CodeSystem,
                    DisplayName = documentEntry.PracticeSettingCode?.DisplayName
                },
                RepositoryUniqueId = documentEntry.RepositoryUniqueId,
                ServiceStartTime = documentEntry.ServiceStartTime,
                ServiceStopTime = documentEntry.ServiceStopTime,
                Size = documentEntry.Size,
                SourcePatientInfo = new()
                {
                    PatientId = new()
                    {
                        Id = documentEntry.SourcePatientInfo?.PatientId,
                        System = documentEntry.SourcePatientInfo?.PatientSystem
                    },
                    FirstName = documentEntry.SourcePatientInfo?.FirstName,
                    LastName = documentEntry.SourcePatientInfo?.LastName,
                    BirthTime = documentEntry.SourcePatientInfo?.BirthTime,
                    Gender = documentEntry.SourcePatientInfo?.Gender,
                },
                Title = documentEntry.Title,
                TypeCode = new()
                {
                    Code = documentEntry.TypeCode?.Code,
                    CodeSystem = documentEntry.TypeCode?.CodeSystem,
                    DisplayName = documentEntry.TypeCode?.DisplayName
                },
                UniqueId = documentEntry.UniqueId
            });
        }

        foreach (var submissionSet in registryObjects?.OfType<DbSubmissionSet>() ?? [])
        {
            registryObjectDtos.Add(new SubmissionSetDto()
            {
                Author = submissionSet.Author.Select(a => new AuthorInfo()
                {
                    Organization = new()
                    {
                        Id = a.OrganizationId,
                        OrganizationName = a.OrganizationName,
                        AssigningAuthority = a.OrganizationAssigningAuthority
                    },
                    Department = new()
                    {
                        Id = a.DepartmentId,
                        OrganizationName = a.DepartmentName,
                        AssigningAuthority = a.DepartmentAssigningAuthority
                    },
                    Person = new()
                    {
                        Id = a.PersonId,
                        AssigningAuthority = a.PersonAssigningAuthority,
                        FirstName = a.PersonFirstName,
                        LastName = a.PersonLastName
                    },
                    Role = new()
                    {
                        Code = a.RoleCode,
                        CodeSystem = a.RoleCodeSystem,
                        DisplayName = a.RoleDisplayName
                    },
                    Speciality = new()
                    {
                        Code = a.SpecialityCode,
                        CodeSystem = a.SpecialityCodeSystem,
                        DisplayName = a.SpecialityDisplayName
                    }
                }).ToList(),

                AvailabilityStatus = submissionSet.AvailabilityStatus,
                HomeCommunityId = submissionSet.HomeCommunityId,
                Id = submissionSet.Id,
                Title = submissionSet.Title,
                UniqueId = submissionSet.UniqueId
            });
        }

        foreach (var association in registryObjects?.OfType<DbAssociation>() ?? [])
        {
            registryObjectDtos.Add(new AssociationDto()
            {
                Id = association.Id,
                AssociationType = association.AssociationType,
                SourceObject = association.SourceObjectId,
                TargetObject = association.TargetObjectId,
                SubmissionSetStatus = association.SubmissionSetStatus
            });
        }

        return registryObjectDtos;
    }

    public static DbRegistryObject? MapFromDtoToDatabaseEntity(RegistryObjectDto registryObjectDtos)
    {
        return MapFromDtoToDatabaseEntity([registryObjectDtos]).FirstOrDefault();
    }

    public static List<DbRegistryObject> MapFromDtoToDatabaseEntity(List<RegistryObjectDto> registryObjectDtos)
    {
        var registryObjects = new List<DbRegistryObject>();
        if (registryObjectDtos?.Count == 0) return registryObjects;

        foreach (var documentEntryDto in registryObjectDtos?.OfType<DocumentEntryDto>() ?? [])
        {
            registryObjects.Add(new DbDocumentEntry()
            {
                Author = documentEntryDto.Author?.Select(a => new DbAuthorInfo()
                {
                    DepartmentId = a.Department?.Id,
                    DepartmentAssigningAuthority = a.Department?.AssigningAuthority,
                    DepartmentName = a.Department?.OrganizationName,

                    OrganizationId = a.Organization?.Id,
                    OrganizationAssigningAuthority = a.Organization?.AssigningAuthority,
                    OrganizationName = a.Organization?.OrganizationName,

                    PersonId = a.Person?.Id,
                    PersonAssigningAuthority = a.Person?.AssigningAuthority,
                    PersonFirstName = a.Person?.FirstName,
                    PersonLastName = a.Person?.LastName,

                    RoleCode = a.Role?.Code,
                    RoleCodeSystem = a.Role?.CodeSystem,
                    RoleDisplayName = a.Role?.DisplayName,

                    SpecialityCode = a.Speciality?.Code,
                    SpecialityCodeSystem = a.Speciality?.CodeSystem,
                    SpecialityDisplayName = a.Speciality?.DisplayName,
                }).ToList() ?? [],

                AvailabilityStatus = documentEntryDto.AvailabilityStatus,
                ClassCode = new()
                {
                    Code = documentEntryDto.ClassCode?.Code,
                    CodeSystem = documentEntryDto.ClassCode?.CodeSystem,
                    DisplayName = documentEntryDto.ClassCode?.DisplayName
                },
                ConfidentialityCode = documentEntryDto.ConfidentialityCode?.Select(c => new DbCodedValue()
                {
                    Code = c.Code,
                    CodeSystem = c.CodeSystem,
                    DisplayName = c.DisplayName,
                }).ToList() ?? [],
                CreationTime = documentEntryDto.CreationTime,
                EventCodeList = new()
                {
                    Code = documentEntryDto.EventCodeList?.Code,
                    CodeSystem = documentEntryDto.EventCodeList?.CodeSystem,
                    DisplayName = documentEntryDto.EventCodeList?.DisplayName
                },
                FormatCode = new()
                {
                    Code = documentEntryDto.FormatCode?.Code,
                    CodeSystem = documentEntryDto.FormatCode?.CodeSystem,
                    DisplayName = documentEntryDto.FormatCode?.DisplayName
                },
                Hash = documentEntryDto.Hash,
                HealthCareFacilityTypeCode = new()
                {
                    Code = documentEntryDto.HealthCareFacilityTypeCode?.Code,
                    CodeSystem = documentEntryDto.HealthCareFacilityTypeCode?.CodeSystem,
                    DisplayName = documentEntryDto.HealthCareFacilityTypeCode?.DisplayName,
                },
                HomeCommunityId = documentEntryDto.HomeCommunityId,
                Id = documentEntryDto.Id,
                LanguageCode = documentEntryDto.LanguageCode,
                LegalAuthenticator = new()
                {
                    Id = documentEntryDto.LegalAuthenticator?.Id,
                    IdSystem = documentEntryDto.LegalAuthenticator?.IdSystem,
                    FirstName = documentEntryDto.LegalAuthenticator?.FirstName,
                    LastName = documentEntryDto.LegalAuthenticator?.LastName
                },
                MimeType = documentEntryDto.MimeType,
                ObjectType = documentEntryDto.ObjectType,
                PracticeSettingCode = new()
                {
                    Code = documentEntryDto.PracticeSettingCode?.Code,
                    CodeSystem = documentEntryDto.PracticeSettingCode?.CodeSystem,
                    DisplayName = documentEntryDto.PracticeSettingCode?.DisplayName
                },
                RepositoryUniqueId = documentEntryDto.RepositoryUniqueId,
                ServiceStartTime = documentEntryDto.ServiceStartTime,
                ServiceStopTime = documentEntryDto.ServiceStopTime,
                Size = documentEntryDto.Size,
                SourcePatientInfo = new()
                {
                    PatientId = documentEntryDto.SourcePatientInfo?.PatientId?.Id,
                    PatientSystem = documentEntryDto.SourcePatientInfo?.PatientId?.System,
                    FirstName = documentEntryDto.SourcePatientInfo?.FirstName,
                    LastName = documentEntryDto.SourcePatientInfo?.LastName,
                    BirthTime = documentEntryDto.SourcePatientInfo?.BirthTime,
                    Gender = documentEntryDto.SourcePatientInfo?.Gender,
                },
                Title = documentEntryDto.Title,
                TypeCode = new()
                {
                    Code = documentEntryDto.TypeCode?.Code,
                    CodeSystem = documentEntryDto.TypeCode?.CodeSystem,
                    DisplayName = documentEntryDto.TypeCode?.DisplayName,
                },
                UniqueId = documentEntryDto.UniqueId
            });
        }

        foreach (var submissionSetDto in registryObjectDtos?.OfType<SubmissionSetDto>() ?? [])
        {
            registryObjects.Add(new DbSubmissionSet()
            {
                Author = submissionSetDto.Author?.Select(a => new DbAuthorInfo()
                {
                    DepartmentId = a.Department?.Id,
                    DepartmentAssigningAuthority = a.Department?.AssigningAuthority,
                    DepartmentName = a.Department?.OrganizationName,

                    OrganizationId = a.Organization?.Id,
                    OrganizationAssigningAuthority = a.Organization?.AssigningAuthority,
                    OrganizationName = a.Organization?.OrganizationName,

                    PersonId = a.Person?.Id,
                    PersonAssigningAuthority = a.Person?.AssigningAuthority,
                    PersonFirstName = a.Person?.FirstName,
                    PersonLastName = a.Person?.LastName,

                    RoleCode = a.Role?.Code,
                    RoleCodeSystem = a.Role?.CodeSystem,
                    RoleDisplayName = a.Role?.DisplayName,

                    SpecialityCode = a.Speciality?.Code,
                    SpecialityCodeSystem = a.Speciality?.CodeSystem,
                    SpecialityDisplayName = a.Speciality?.DisplayName,
                }).ToList() ?? [],

                AvailabilityStatus = submissionSetDto.AvailabilityStatus,
                HomeCommunityId = submissionSetDto.HomeCommunityId,
                Id = submissionSetDto.Id,
                Title = submissionSetDto.Title,
                UniqueId = submissionSetDto.UniqueId,
                SourceId = submissionSetDto.SourceId,
                SubmissionTime = submissionSetDto.SubmissionTime
            });
        }

        foreach (var associationDto in registryObjectDtos?.OfType<AssociationDto>() ?? [])
        {
            registryObjects.Add(new DbAssociation()
            {
                Id = associationDto.Id,
                AssociationType = associationDto.AssociationType,
                SourceObjectId = associationDto.SourceObject,
                TargetObjectId = associationDto.TargetObject,
                SubmissionSetStatus = associationDto.SubmissionSetStatus
            });
        }

        return registryObjects;
    }
}