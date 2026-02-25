using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Tests.Helpers;
using XcaXds.WebService;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

#pragma warning disable CS8604, CS8602 // Possible null reference argument.
public class IntegrationTests_FhirMobileAccessToHealthDocuments : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_FhirMobileAccessToHealthDocuments(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata()
    {
        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "DEFAULT_machine_deletedocuments",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: "nhn:phr/mhd/create-documents-with-reference",
            action: "Delete",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var registryObjects = RegistryContent.AsRegistryObjectList();

        var registryContentCount = registryObjects.Count;

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}");

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var currentRegistry = _registry.ReadRegistry();
        var currentCount = currentRegistry.Count();

        Assert.Equal(registryContentCount - 3, currentCount);
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);
        _output.WriteLine("DeleteDocumentsAndMetadata: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Patch", "Patch DocumentReference securityLabel")]
    public async Task PatchDocumentSecurityLabel_ExportsAtnaLog()
    {
        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "DEFAULT_machine_patchdocumentreference",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: "nhn:phr/mhd/create-documents-with-reference",
            action: "Update",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var patchBody = """
        {
          "securityLabel": [
            {
              "coding": [
                {
                  "system": "http://example.org/security",
                  "code": "N",
                  "display": "Normal"
                }
              ]
            }
          ]
        }
        """;

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}")
        {
            Content = new StringContent(patchBody, Encoding.UTF8, Constants.MimeTypes.FhirJson)
        };
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Audit is generated via background service; allow a brief window for the queue to be drained.
        var timeoutAt = DateTime.UtcNow.AddSeconds(2);
        while (!_atnaLogExportedChecker.AtnaLogExported && DateTime.UtcNow < timeoutAt)
        {
            await Task.Delay(50);
        }

        Assert.True(_atnaLogExportedChecker.AtnaLogExported);
        _output.WriteLine("PatchDocumentSecurityLabel_ExportsAtnaLog: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle")]
    public async Task ProvideBundle_RandomAmount()
    {
        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "DEFAULT_machine_providebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: "nhn:phr/mhd/create-documents-with-reference",
            action: "Create",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var responseContent = await firstResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    private List<DocumentReferenceDto> EnsureRegistryAndRepositoryHasContent(int registryObjectsCount = 10, string? patientIdentifier = null)
    {
        var metadata = TestHelpers.GenerateRegistryMetadata(registryObjectsCount, patientIdentifier, true);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(metadata.AsRegistryObjectList());

        foreach (var document in metadata.Select(dto => dto.Document))
        {
            _repository.Write(document.DocumentId, document.Data);
        }

        return metadata;
    }

}
#pragma warning restore CS8604, CS8602 // Possible null reference argument.
