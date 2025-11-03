using System.Security.Cryptography;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Source.Source;

namespace XcaXds.Commons.Services;

public static class RegistryMetadataGeneratorService
{
    public static List<RegistryObjectDto> GenerateRandomizedTestData(ApplicationConfig applicationConfig, Test_DocumentReference jsonTestData, RepositoryWrapper repositoryWrapper, int? entriesToGenerate = 0, string? patientIdentifier = null)
    {
        jsonTestData.PossibleSubmissionSetValues.Authors ??= jsonTestData.PossibleDocumentEntryValues.Authors;

        entriesToGenerate = entriesToGenerate == 0 ? 10 : entriesToGenerate;

        var sourcePatientInfoForPatient = jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos.FirstOrDefault(spi => spi?.PatientId?.Id == patientIdentifier);

        if (sourcePatientInfoForPatient != null)
        {
            jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos = [sourcePatientInfoForPatient];
        }
        var generatedTestRegistryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, (int)entriesToGenerate!);

        var files = jsonTestData.Documents.Select(file => Convert.FromBase64String(file));

        foreach (var generatedTestObject in generatedTestRegistryObjects.OfType<DocumentEntryDto>())
        {
            var randomFileAsByteArray = files.ElementAt(Random.Shared.Next(files.Count()));

            if (generatedTestObject?.SourcePatientInfo?.PatientId?.Id != null && generatedTestObject.Id != null && randomFileAsByteArray != null)
            {
                generatedTestObject.Title = "XcaDS - " + generatedTestObject.Title;
                generatedTestObject.Size = randomFileAsByteArray.Length.ToString();
                generatedTestObject.Hash = BitConverter.ToString(SHA1.HashData(randomFileAsByteArray)).Replace("-", "").ToLowerInvariant();
                generatedTestObject.RepositoryUniqueId = applicationConfig.RepositoryUniqueId;
                generatedTestObject.HomeCommunityId = applicationConfig.HomeCommunityId;

                repositoryWrapper.StoreDocument(generatedTestObject.UniqueId, randomFileAsByteArray, generatedTestObject.SourcePatientInfo.PatientId.Id);
            }
        }
        return generatedTestRegistryObjects;
    }
}
