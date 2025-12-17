using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
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

    public static List<RegistryObjectDto> GenerateRegistryMetadata()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var data =  File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("TestDataRegistryObjects.json")));

        return RegistryMetadataGeneratorService.GenerateRandomizedTestData("2.16.578.1.12.4.5.100.1.1", "2.16.578.1.12.4.5.100.1.1.2", JsonSerializer.Deserialize<Test_DocumentReference>(data, Constants.JsonDefaultOptions.DefaultSettings));
    }
}
