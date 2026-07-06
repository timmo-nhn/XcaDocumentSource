using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Shared.Models.Custom;
using XcaXds.Source.Models.DatabaseDtos;
using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Source;

public static class DatabaseMapper
{
    public static IEnumerable<RegistryObjectDto> MapFromDatabaseEntityToDto(IEnumerable<DbRegistryObject> registryObjects)
    {
        var registryObjectDtos = new List<RegistryObjectDto>();
        if (registryObjects == null) yield break;

        foreach (var registryObject in registryObjects)
        {
            var dtoObject = MapFromDatabaseEntityToDto(registryObject);
            if (dtoObject == null) continue;

            yield return dtoObject;
        }
    }

    public static RegistryObjectDto MapFromDatabaseEntityToDto(DbRegistryObject registryObject)
    {
        switch (registryObject)
        {
            case DbDocumentEntry documentEntry:
                return new DocumentEntryDto()
                {
                    Author = documentEntry.DE_Author?.Select(a => new AuthorInfo()
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

                    AvailabilityStatus = documentEntry.DE_AvailabilityStatus,
                    ClassCode = new()
                    {
                        Code = documentEntry.DE_ClassCode?.Code,
                        CodeSystem = documentEntry.DE_ClassCode?.CodeSystem,
                        DisplayName = documentEntry.DE_ClassCode?.DisplayName
                    },
                    ConfidentialityCode = documentEntry.DE_ConfidentialityCode?.Select(c => new CodedValue()
                    {
                        Code = c.Code,
                        CodeSystem = c.CodeSystem,
                        DisplayName = c.DisplayName
                    }).ToList(),

                    CreationTime = documentEntry.DE_CreationTime,
                    EventCodeList = new()
                    {
                        Code = documentEntry.DE_EventCodeList?.Code,
                        CodeSystem = documentEntry.DE_EventCodeList?.CodeSystem,
                        DisplayName = documentEntry.DE_EventCodeList?.DisplayName
                    },
                    FormatCode = new()
                    {
                        Code = documentEntry.DE_FormatCode?.Code,
                        CodeSystem = documentEntry.DE_FormatCode?.CodeSystem,
                        DisplayName = documentEntry.DE_FormatCode?.DisplayName,
                    },
                    Hash = documentEntry.DE_Hash,
                    HealthCareFacilityTypeCode = new()
                    {
                        Code = documentEntry.DE_HealthCareFacilityTypeCode?.Code,
                        CodeSystem = documentEntry.DE_HealthCareFacilityTypeCode?.CodeSystem,
                        DisplayName = documentEntry.DE_HealthCareFacilityTypeCode?.DisplayName,
                    },
                    HomeCommunityId = documentEntry.DE_HomeCommunityId,
                    LanguageCode = documentEntry.DE_LanguageCode,
                    LegalAuthenticator = new()
                    {
                        Id = documentEntry.DE_LegalAuthenticator?.Id,
                        IdSystem = documentEntry.DE_LegalAuthenticator?.IdSystem,
                        FirstName = documentEntry.DE_LegalAuthenticator?.FirstName,
                        LastName = documentEntry.DE_LegalAuthenticator?.LastName,
                    },
                    Id = documentEntry.Id ?? "Unknown",
                    MimeType = documentEntry.DE_MimeType,
                    ObjectType = documentEntry.DE_ObjectType,
                    PracticeSettingCode = new()
                    {
                        Code = documentEntry.DE_PracticeSettingCode?.Code,
                        CodeSystem = documentEntry.DE_PracticeSettingCode?.CodeSystem,
                        DisplayName = documentEntry.DE_PracticeSettingCode?.DisplayName
                    },
                    RepositoryUniqueId = documentEntry.DE_RepositoryUniqueId,
                    ServiceStartTime = documentEntry.DE_ServiceStartTime,
                    ServiceStopTime = documentEntry.DE_ServiceStopTime,
                    Size = documentEntry.DE_Size,
                    SourcePatientInfo = new()
                    {
                        PatientId = new()
                        {
                            Id = documentEntry.DE_SourcePatientInfoPatientId,
                            System = documentEntry.DE_SourcePatientInfoPatientSystem
                        },
                        FirstName = documentEntry.DE_SourcePatientInfoFirstName,
                        LastName = documentEntry.DE_SourcePatientInfoLastName,
                        BirthTime = documentEntry.DE_SourcePatientInfoBirthTime,
                        Gender = documentEntry.DE_SourcePatientInfoGender
                    },
                    Title = documentEntry.DE_Title,
                    TypeCode = new()
                    {
                        Code = documentEntry.DE_TypeCode?.Code,
                        CodeSystem = documentEntry.DE_TypeCode?.CodeSystem,
                        DisplayName = documentEntry.DE_TypeCode?.DisplayName
                    },
                    UniqueId = documentEntry.DE_UniqueId
                };

            case DbSubmissionSet submissionSet:
                return new SubmissionSetDto()
                {
                    Author = submissionSet.SS_Author.Select(a => new AuthorInfo()
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

                    AvailabilityStatus = submissionSet.SS_AvailabilityStatus,
                    HomeCommunityId = submissionSet.SS_HomeCommunityId,
                    Id = submissionSet.Id ?? throw new InvalidOperationException("Submissionset id is null!"),
                    Title = submissionSet.SS_Title,
                    UniqueId = submissionSet.SS_UniqueId
                };


            case DbAssociation association:
                return new AssociationDto()
                {
                    Id = association.Id ?? throw new InvalidOperationException("Submissionset id is null!"),
                    AssociationType = association.AS_AssociationType,
                    SourceObject = association.AS_SourceObjectId,
                    TargetObject = association.AS_TargetObjectId,
                    SubmissionSetStatus = association.AS_SubmissionSetStatus
                };

            default:
                throw new InvalidOperationException($"Unknown entity type ({registryObject.GetType().Name})");
        }
    }

    public static DbRegistryObject? MapFromDtoToDatabaseEntity(RegistryObjectDto registryObjectDto)
    {
        if (registryObjectDto is DocumentEntryDto documentEntryDto)
        {
            if (string.IsNullOrWhiteSpace(documentEntryDto.SourcePatientInfo?.PatientId?.Id))
            {
                throw new InvalidOperationException("Patient Id cannot be null");
            }
            if (string.IsNullOrWhiteSpace(documentEntryDto.SourcePatientInfo?.PatientId?.System))
            {
                throw new InvalidOperationException("Patient System cannot be null");
            }

            return new DbDocumentEntry()
            {
                DE_Author = documentEntryDto.Author?.Select(a => new DbAuthorInfo()
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

                DE_AvailabilityStatus = documentEntryDto.AvailabilityStatus,
                DE_ClassCode = new()
                {
                    Code = documentEntryDto.ClassCode?.Code,
                    CodeSystem = documentEntryDto.ClassCode?.CodeSystem,
                    DisplayName = documentEntryDto.ClassCode?.DisplayName
                },
                DE_ConfidentialityCode = documentEntryDto.ConfidentialityCode?.Select(c => new DbCodedValue()
                {
                    Code = c.Code,
                    CodeSystem = c.CodeSystem,
                    DisplayName = c.DisplayName,
                }).ToList() ?? [],
                DE_CreationTime = EnsureUtc(documentEntryDto.CreationTime),
                DE_EventCodeList = new()
                {
                    Code = documentEntryDto.EventCodeList?.Code,
                    CodeSystem = documentEntryDto.EventCodeList?.CodeSystem,
                    DisplayName = documentEntryDto.EventCodeList?.DisplayName
                },
                DE_FormatCode = new()
                {
                    Code = documentEntryDto.FormatCode?.Code,
                    CodeSystem = documentEntryDto.FormatCode?.CodeSystem,
                    DisplayName = documentEntryDto.FormatCode?.DisplayName
                },
                DE_Hash = documentEntryDto.Hash,
                DE_HealthCareFacilityTypeCode = new()
                {
                    Code = documentEntryDto.HealthCareFacilityTypeCode?.Code,
                    CodeSystem = documentEntryDto.HealthCareFacilityTypeCode?.CodeSystem,
                    DisplayName = documentEntryDto.HealthCareFacilityTypeCode?.DisplayName,
                },
                DE_HomeCommunityId = documentEntryDto.HomeCommunityId,
                Id = documentEntryDto.Id,
                DE_LanguageCode = documentEntryDto.LanguageCode,
                DE_LegalAuthenticator = new()
                {
                    Id = documentEntryDto.LegalAuthenticator?.Id,
                    IdSystem = documentEntryDto.LegalAuthenticator?.IdSystem,
                    FirstName = documentEntryDto.LegalAuthenticator?.FirstName,
                    LastName = documentEntryDto.LegalAuthenticator?.LastName
                },
                DE_MimeType = documentEntryDto.MimeType,
                DE_ObjectType = documentEntryDto.ObjectType,
                DE_PracticeSettingCode = new()
                {
                    Code = documentEntryDto.PracticeSettingCode?.Code,
                    CodeSystem = documentEntryDto.PracticeSettingCode?.CodeSystem,
                    DisplayName = documentEntryDto.PracticeSettingCode?.DisplayName
                },
                DE_RepositoryUniqueId = documentEntryDto.RepositoryUniqueId,
                DE_ServiceStartTime = EnsureUtc(documentEntryDto.ServiceStartTime),
                DE_ServiceStopTime = EnsureUtc(documentEntryDto.ServiceStopTime),
                DE_Size = documentEntryDto.Size,
                DE_SourcePatientInfoPatientId = documentEntryDto.SourcePatientInfo.PatientId.Id,
                DE_SourcePatientInfoPatientSystem = documentEntryDto.SourcePatientInfo.PatientId.System,
                DE_SourcePatientInfoFirstName = documentEntryDto.SourcePatientInfo?.FirstName,
                DE_SourcePatientInfoLastName = documentEntryDto.SourcePatientInfo?.LastName,
                DE_SourcePatientInfoBirthTime = EnsureUtc(documentEntryDto.SourcePatientInfo?.BirthTime),
                DE_SourcePatientInfoGender = documentEntryDto.SourcePatientInfo?.Gender,
                DE_Title = documentEntryDto.Title,
                DE_TypeCode = new()
                {
                    Code = documentEntryDto.TypeCode?.Code,
                    CodeSystem = documentEntryDto.TypeCode?.CodeSystem,
                    DisplayName = documentEntryDto.TypeCode?.DisplayName,
                },
                DE_UniqueId = documentEntryDto.UniqueId
            };
        }

        if (registryObjectDto is SubmissionSetDto submissionSetDto)
        {
            return new DbSubmissionSet()
            {
                SS_Author = submissionSetDto.Author?.Select(a => new DbAuthorInfo()
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

                SS_AvailabilityStatus = submissionSetDto.AvailabilityStatus,
                SS_HomeCommunityId = submissionSetDto.HomeCommunityId,
                Id = submissionSetDto.Id,
                SS_Title = submissionSetDto.Title,
                SS_UniqueId = submissionSetDto.UniqueId,
                SS_SourceId = submissionSetDto.SourceId,
                SS_SubmissionTime = EnsureUtc(submissionSetDto.SubmissionTime)
            };
        }

        if (registryObjectDto is AssociationDto associationDto)
        {
            return new DbAssociation()
            {
                Id = associationDto.Id,
                AS_AssociationType = associationDto.AssociationType,
                AS_SourceObjectId = associationDto.SourceObject,
                AS_TargetObjectId = associationDto.TargetObject,
                AS_SubmissionSetStatus = associationDto.SubmissionSetStatus
            };
        }

        return null;
    }

    public static IEnumerable<DbRegistryObject> MapFromDtoToDatabaseEntity(IEnumerable<RegistryObjectDto> registryObjectDtos)
    {
        var registryObjects = new List<DbRegistryObject>();
        if (registryObjectDtos == null) yield break;

        foreach (var documentEntryDto in registryObjectDtos)
        {
            var databaseEntity = MapFromDtoToDatabaseEntity(documentEntryDto);

            if (databaseEntity == null) continue;

            yield return databaseEntity;
        }
    }

    private static DateTime? EnsureUtc(DateTime? value)
    {
        if (value == null) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }
}