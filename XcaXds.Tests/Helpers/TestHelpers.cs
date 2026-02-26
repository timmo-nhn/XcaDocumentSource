using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.DataManipulators;
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
        catch (Exception ex)
        {
            return null;
        }
    }

    public static List<DocumentReferenceDto> GenerateComprehensiveRegistryMetadata(string? patientId = null, bool noDeprecatedDocuments = false)
    {
        return GenerateRegistryMetadata(1, patientId, noDeprecatedDocuments);
    }

    public static List<DocumentReferenceDto> GenerateRegistryMetadata(int amount = 10, string? patientId = null, bool noDeprecatedDocuments = false)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var data =  File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("TestDataRegistryObjects.json")) ?? "");

        return RegistryMetadataGenerator.GenerateRandomizedTestData(
            homeCommunityId:"2.16.578.1.12.4.5.100.1.1", 
            repositoryUniqueId:"2.16.578.1.12.4.5.100.1.1.2", 
            jsonTestData: JsonSerializer.Deserialize<Test_DocumentReference>(data, Constants.JsonDefaultOptions.DefaultSettings),
            entriesToGenerate:amount,
            patientIdentifier: patientId,
            noDeprecatedDocuments: noDeprecatedDocuments);
    }

    public static void AddAccessControlPolicyForIntegrationTest(PolicyRepositoryService policyRepositoryService, string policyName, string attributeId, string codeValue,string action, string? codeSystemValue = null, bool noCode = false)
    {
        policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [Issuer.HelseId, Issuer.Helsenorge],
            Id = policyName,
            Rules =
            [[
                new() { AttributeId = attributeId + $"{(noCode ? string.Empty: ":code")}", Value = codeValue },
                codeSystemValue == null ? null : new() { AttributeId = attributeId + ":codeSystem", Value = codeSystemValue }
            ]],
            Actions = [action],
            Effect = "Permit",
        });
    }

}
