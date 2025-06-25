using System.Security.Cryptography;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;

namespace XcaXds.Commons.Services;

public static class TestDataGeneratorService
{
    public static List<RegistryObjectDto> GenerateRegistryObjectsFromTestData(Test_DocumentReference documentEntryValues, int amount)
    {
        var registryObjects = new List<RegistryObjectDto>();

        int maxCount = 0;

        while (maxCount < amount)
        {
            var rng = new Random();
            var creationTime =
                DateTime.UtcNow.AddSeconds(-rng.Next(1_000_000_000));


            var documentEntry = new DocumentEntryDto()
            {
                Author = new()
                {
                    Person = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Persons),
                    Department = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Departments),
                    Organization = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Organizations),
                    Role = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Roles),
                    Speciality = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Specialities),
                },
                AvailabilityStatus = PickRandom(documentEntryValues.PossibleDocumentEntryValues.AvailabilityStatuses),
                ClassCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.ClassCodes),
                ConfidentialityCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.ConfidentialityCodes),
                CreationTime = creationTime.AddSeconds(-rng.Next(10_000)),
                EventCodeList = PickRandom(documentEntryValues.PossibleDocumentEntryValues.EventCodeLists),
                FormatCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.FormatCodes),
                Hash = SHA256.Create().ToString(),
                HealthCareFacilityTypeCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.HealthcareFacilityTypeCodes),
                HomeCommunityId = PickRandom(documentEntryValues.PossibleDocumentEntryValues.HomeCommunityIds),
                Id = Guid.NewGuid().ToString(),
                LanguageCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.LanguageCodes),
                LegalAuthenticator = PickRandom(documentEntryValues.PossibleDocumentEntryValues.LegalAuthenticators),
                MimeType = PickRandom(documentEntryValues.PossibleDocumentEntryValues.MimeTypes),
                ObjectType = PickRandom(documentEntryValues.PossibleDocumentEntryValues.ObjectTypes),
                PatientId = PickRandom(documentEntryValues.PossibleDocumentEntryValues.PatientIdentifiers),
                PracticeSettingCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.PracticeSettingCodes),
                RepositoryUniqueId = PickRandom(documentEntryValues.PossibleDocumentEntryValues.RepositoryUniqueIds),
                ServiceStartTime = creationTime.AddSeconds(-rng.Next(10_000, 20_000)),
                ServiceStopTime = creationTime.AddSeconds(-rng.Next(10_000)),
                Size = "32768",
                SourcePatientInfo = PickRandom(documentEntryValues.PossibleDocumentEntryValues.SourcePatientInfos),
                Title = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Titles),
                TypeCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.TypeCodes),
                UniqueId = Guid.NewGuid().ToString()
            };

            var submissionSet = new SubmissionSetDto()
            {
                Author = new()
                {
                    Person = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Persons),
                    Department = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Departments),
                    Organization = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Organizations),
                    Role = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Roles),
                    Speciality = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Authors.Specialities),
                },
                AvailabilityStatus = PickRandom(documentEntryValues.PossibleDocumentEntryValues.AvailabilityStatuses),
                HomeCommunityId = documentEntry.HomeCommunityId,
                Id = Guid.NewGuid().ToString(),
                PatientId = documentEntry.PatientId,
                SubmissionTime = creationTime.AddSeconds(-rng.Next(1000)),
                SourceId = documentEntry.HomeCommunityId + $".{new Random().Next(4096)}.{new Random().Next(4096)}.{new Random().Next(4096)}",
                UniqueId = documentEntry.HomeCommunityId + $".{new Random().Next(4096)}.{new Random().Next(4096)}.{new Random().Next(4096)}",
                Title = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Titles)
            };

            var associaiton = new AssociationDto()
            {
                AssociationType = Constants.Xds.AssociationType.HasMember,
                SourceObject = submissionSet.Id,
                SubmissionSetStatus = "Current",
                TargetObject = documentEntry.Id,

            };

            registryObjects.Add(documentEntry);
            registryObjects.Add(submissionSet);
            registryObjects.Add(associaiton);
            maxCount++;
        }

        return registryObjects;
    }

    public static void UploadFilesFromRegistryObjectsAndfiles(List<byte[]> files, List<RegistryObjectDto> registryObjects)
    {
        throw new NotImplementedException();
    }

    internal static T PickRandom<T>(IEnumerable<T> inputs)
    {
        if (inputs == null)
        {
            return default;
        }
        var random = new Random();
        return inputs.ElementAt(random.Next(inputs.Count()));
    }
}
