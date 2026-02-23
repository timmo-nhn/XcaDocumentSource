using System.Security.Cryptography;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;

namespace XcaXds.Commons.DataManipulators;

public static class RegistryMetadataGenerator
{
    public static List<DocumentReferenceDto> GenerateRandomizedTestData(
        string? homeCommunityId,
        string? repositoryUniqueId,
        Test_DocumentReference? jsonTestData,
        int? entriesToGenerate = 10,
        string? patientIdentifier = null,
        bool noDeprecatedDocuments = false
        )
    {
        if (jsonTestData?.PossibleDocumentEntryValues == null) return new();

        if (noDeprecatedDocuments == true)
        {
            jsonTestData.PossibleDocumentEntryValues.AvailabilityStatuses = [Constants.Xds.StatusValues.Approved];
        }

        jsonTestData.PossibleSubmissionSetValues.Authors ??= jsonTestData.PossibleDocumentEntryValues.Authors;

        var sourcePatientInfoForPatient = jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos.FirstOrDefault(spi => spi?.PatientId?.Id == patientIdentifier);

        if (sourcePatientInfoForPatient != null)
        {
            jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos = [sourcePatientInfoForPatient];
        }

        var generatedTestRegistryObjects = TestDataGenerator.GenerateRegistryObjectsFromTestData(jsonTestData, (int)entriesToGenerate!);

        foreach (var generatedTestObject in generatedTestRegistryObjects)
        {
            var documentContent = generatedTestObject.Document?.Data;

            if (generatedTestObject?.DocumentEntry?.SourcePatientInfo?.PatientId?.Id != null && generatedTestObject.DocumentEntry.Id != null && documentContent != null)
            {
                generatedTestObject.DocumentEntry.Title = "XcaDS - " + generatedTestObject.DocumentEntry.Title;
                generatedTestObject.DocumentEntry.Size = documentContent.Length.ToString();
                generatedTestObject.DocumentEntry.Hash = BitConverter.ToString(SHA1.HashData(documentContent)).Replace("-", "").ToLowerInvariant();
                generatedTestObject.DocumentEntry.HomeCommunityId = homeCommunityId;
                generatedTestObject.DocumentEntry.RepositoryUniqueId = repositoryUniqueId;
            }


        }
        return generatedTestRegistryObjects;
    }
}
