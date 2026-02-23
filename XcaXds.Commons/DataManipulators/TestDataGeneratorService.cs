using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;

namespace XcaXds.Commons.DataManipulators;

public static class TestDataGeneratorService
{
    public static List<DocumentReferenceDto> GenerateRegistryObjectsFromTestData(Test_DocumentReference documentEntryValues, int amount)
    {
        var registryObjects = new List<DocumentReferenceDto>();

        int maxCount = 0;

        while (maxCount < amount)
        {
            var rng = new Random();
            var creationTime = DateTime.UtcNow.AddSeconds(-rng.Next(100_000_000));

            var documentEntry = new DocumentEntryDto()
            {
                Author = GetRandomAuthors(documentEntryValues.PossibleDocumentEntryValues.Authors, Random.Shared.Next(3) + 1)?.Distinct().ToList(),
                AvailabilityStatus = PickRandom(documentEntryValues.PossibleDocumentEntryValues.AvailabilityStatuses),
                ClassCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.ClassCodes),
                ConfidentialityCode = PickRandomAmount(documentEntryValues.PossibleDocumentEntryValues.ConfidentialityCodes, Random.Shared.Next(3) + 1)?.Distinct().ToList(),
                CreationTime = creationTime.AddSeconds(-rng.Next(10_000)),
                EventCodeList = PickRandom(documentEntryValues.PossibleDocumentEntryValues.EventCodeLists),
                FormatCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.FormatCodes),
                HealthCareFacilityTypeCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.HealthcareFacilityTypeCodes),
                HomeCommunityId = PickRandom(documentEntryValues.PossibleDocumentEntryValues.HomeCommunityIds),
                Id = Guid.NewGuid().ToString(),
                LanguageCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.LanguageCodes),
                LegalAuthenticator = PickRandom(documentEntryValues.PossibleDocumentEntryValues.LegalAuthenticators),
                MimeType = PickRandom(documentEntryValues.PossibleDocumentEntryValues.MimeTypes),
                ObjectType = PickRandom(documentEntryValues.PossibleDocumentEntryValues.ObjectTypes),
                PracticeSettingCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.PracticeSettingCodes),
                RepositoryUniqueId = PickRandom(documentEntryValues.PossibleDocumentEntryValues.RepositoryUniqueIds),
                ServiceStartTime = creationTime.AddSeconds(-rng.Next(10_000, 20_000)),
                ServiceStopTime = creationTime.AddSeconds(-rng.Next(10_000)),
                SourcePatientInfo = PickRandom(documentEntryValues.PossibleDocumentEntryValues.SourcePatientInfos),
                Title = PickRandom(documentEntryValues.PossibleDocumentEntryValues.Titles),
                TypeCode = PickRandom(documentEntryValues.PossibleDocumentEntryValues.TypeCodes),
                UniqueId = Guid.NewGuid().ToString()
            };

            var submissionSet = new SubmissionSetDto()
            {
                Author = GetRandomAuthors(documentEntryValues.PossibleDocumentEntryValues.Authors, Random.Shared.Next(3) + 1)?.Distinct().ToList(),
                AvailabilityStatus = PickRandom(documentEntryValues.PossibleDocumentEntryValues.AvailabilityStatuses),
                HomeCommunityId = documentEntry.HomeCommunityId,
                Id = Guid.NewGuid().ToString(),
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

            var documents = documentEntryValues.Documents.Select(file => Convert.FromBase64String(file));

            var document = new DocumentDto()
            {
                DocumentId = documentEntry.UniqueId,
                Data = documents.ElementAt(Random.Shared.Next(documents.Count()))
            };

            registryObjects.Add(new()
            {
                DocumentEntry = documentEntry,
                Association = associaiton,
                SubmissionSet = submissionSet,
                Document = document
            });

            maxCount++;
        }

        return registryObjects;
    }

    private static List<AuthorInfo>? GetRandomAuthors(TestAuthors authors, int amount)
    {
        var authorInfo = new List<AuthorInfo>();

        for (int i = 0; i < amount; i++)
        {
            authorInfo.Add(new()
            {
                Person = PickRandom(authors.Persons),
                Department = PickRandom(authors.Departments),
                Organization = PickRandom(authors.Organizations),
                Role = PickRandom(authors.Roles),
                Speciality = PickRandom(authors.Specialities),
            });
        }

        return authorInfo.Count == 0 ? null : authorInfo;
    }

    private static IEnumerable<T> PickRandomAmount<T>(IEnumerable<T> inputs, int amount)
    {
        return inputs.PickRandom(amount);
    }

    internal static T PickRandom<T>(IEnumerable<T> inputs)
    {
        if (inputs == null)
        {
            return default;
        }
        return inputs.ElementAt(Random.Shared.Next(inputs.Count()));
    }
}
