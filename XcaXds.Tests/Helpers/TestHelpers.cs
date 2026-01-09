using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Services;

namespace XcaXds.Tests.Helpers;

public static class TestHelpers
{
    public static XmlDocument? LoadNewXmlDocument(string? fileContent)
    {
        if (string.IsNullOrWhiteSpace(fileContent)) return null;
        try
        {
            var document = new XmlDocument();
            document.LoadXml(fileContent);
            return document;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static List<DocumentReferenceDto> GenerateRegistryMetadata(int amount = 10, string? patientId = null, bool noDeprecatedDocuments = false)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var data =  File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("TestDataRegistryObjects.json")));

        return RegistryMetadataGeneratorService.GenerateRandomizedTestData(
            homeCommunityId:"2.16.578.1.12.4.5.100.1.1", 
            repositoryUniqueId:"2.16.578.1.12.4.5.100.1.1.2", 
            jsonTestData: JsonSerializer.Deserialize<Test_DocumentReference>(data, Constants.JsonDefaultOptions.DefaultSettings),
            entriesToGenerate:amount,
            patientIdentifier: patientId,
            noDeprecatedDocuments: noDeprecatedDocuments);
    }
}
