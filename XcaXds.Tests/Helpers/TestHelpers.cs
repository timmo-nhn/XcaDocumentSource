using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.WebService.Services;

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
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generate metadata according to TestDataRegistryObjects.json
    /// </summary>
    public static List<DocumentReferenceDto> GenerateComprehensiveRegistryMetadata(int amount = 10, string? patientId = null, bool noDeprecatedDocuments = false)
    {
        return GenerateRegistryMetadata("TestDataRegistryObjects.json", amount, patientId, noDeprecatedDocuments);
    }

    /// <summary>
    /// Generate metadata according to TestDataRegistryObjects_PotentialNulls.json, possibly generating faulty metadata with null values in various fields
    /// </summary>
    public static List<DocumentReferenceDto> GeneratePotentiallyFaultyComprehensiveRegistryMetadata(int amount = 10, string? patientId = null, bool noDeprecatedDocuments = false)
    {
        return GenerateRegistryMetadata("TestDataRegistryObjects_PotentialNulls.json", amount, patientId, noDeprecatedDocuments);
    }

    private static List<DocumentReferenceDto> GenerateRegistryMetadata(string fileName, int amount = 10, string? patientId = null, bool noDeprecatedDocuments = false)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var data = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains(fileName)) ?? "");

        return RegistryMetadataGenerator.GenerateRandomizedTestData(
            homeCommunityId: "2.16.578.1.12.4.5.100.1.1",
            repositoryUniqueId: "2.16.578.1.12.4.5.100.1.1.2",
            jsonTestData: JsonSerializer.Deserialize<Test_DocumentReference>(data, Constants.JsonDefaultOptions.DefaultSettings),
            entriesToGenerate: amount,
            patientIdentifier: patientId,
            noDeprecatedDocuments: noDeprecatedDocuments);
    }

    public static void AddAccessControlPolicyForIntegrationTest(PolicyRepositoryService policyRepositoryService, string policyName, string attributeId, string codeValue, string action, string? codeSystemValue = null, bool noCode = false)
    {
        var rules = new List<PolicyMatch>
        {
            new() { AttributeId = attributeId + $"{(noCode ? string.Empty : ":code")}", Value = codeValue }
        };

        if (codeSystemValue != null)
        {
            rules.Add(new() { AttributeId = attributeId + ":codeSystem", Value = codeSystemValue });
        }

        policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [AppliesTo.HelseId, AppliesTo.Helsenorge, AppliesTo.Machine],
            Id = policyName,
            Rules = [rules],
            Actions = [action],
            Effect = "Permit",
        });
    }
}
