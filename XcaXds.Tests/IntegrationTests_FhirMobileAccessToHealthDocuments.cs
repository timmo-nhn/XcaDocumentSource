using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
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
    public async Task DeleteDocumentsAndMetadata_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        var registryObjects = RegistryContent.AsRegistryObjectDtos();

        var registryContentCount = registryObjects.Count();

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}");

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var currentRegistry = _registry.ReadRegistry();
        var currentCount = currentRegistry.Count();

        var expectedCount = registryContentCount - 3;

        Assert.Equal(expectedCount, currentCount);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("DeleteDocumentsAndMetadata: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata_DocumentDoesNotExist_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        var registryObjects = RegistryContent.AsRegistryObjectDtos();

        var registryContentCount = registryObjects.Count();

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var documentEntryThatDoesntExist = Guid.NewGuid().ToString();

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/R4/fhir/DocumentReference/{documentEntryThatDoesntExist}");

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var currentRegistry = _registry.ReadRegistry();
        var currentCount = currentRegistry.Count();

        var expectedCount = registryContentCount;

        Assert.Equal(expectedCount, currentCount);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("DeleteDocumentsAndMetadata: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Patch", "Patch DocumentReference securityLabel")]
    public async Task ProvideBundle_PatchDocumentSecurityLabel_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        await WaitForAtnaLogToBeExported();
        _output.WriteLine("PatchDocumentSecurityLabel_ExportsAtnaLog: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle")]
    public async Task ProvideBundle_WrongValues_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01_WrongValues")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var fhirparser = new FhirJsonDeserializer();

        var operationOutcome = fhirparser.Deserialize<OperationOutcome>(responseContent);

        Assert.NotEmpty(operationOutcome.Issue);
        await WaitForAtnaLogToBeExported();

        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle")]
    public async Task ProvideBundle_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle03.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var expectedCount = RegistryContent.Count + 1;

        var firstResponse = await _client.SendAsync(httpRequest);

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var actualCount = _registry.ReadRegistry().OfType<DocumentEntryDto>()?.Count() ?? 0;

        await NukeRegistryRepository();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCount, actualCount);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle (virus)")]
    public async Task ProvideBundle_Virus_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle03_virus.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var responseContent = await firstResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, firstResponse.StatusCode);

        var fhirparser = new FhirJsonDeserializer();

        var operationOutcome = fhirparser.Deserialize<OperationOutcome>(responseContent);

        Assert.NotEmpty(operationOutcome.Issue);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    [Fact]
    [Trait("Upload", "Validate Bundle")]
    public async Task ProvideBundle_Validate_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

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

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "JWt"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle03.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken03_MachineToMachine")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle/$validate");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest);

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var fhirparser = new FhirJsonDeserializer();

        var operationOutcome = fhirparser.Deserialize<OperationOutcome>(responseContent);

        Assert.NotEmpty(operationOutcome.Issue);

        await WaitForAtnaLogToBeExported();
        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString);
    }

    private async Task WaitForAtnaLogToBeExported()
    {
        // Audit is generated via background service; allow a brief window for the queue to be drained.
        var timeoutAt = DateTime.UtcNow.AddSeconds(4);
        while (!_atnaLogExportedChecker.AtnaLogExported && DateTime.UtcNow < timeoutAt)
        {
            await Task.Delay(50);
        }

        Assert.True(_atnaLogExportedChecker.AtnaLogExported);
    }

    private List<DocumentReferenceDto> EnsureRegistryAndRepositoryHasContent(int registryObjectsCount = 10, string? patientIdentifier = null)
    {
        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(registryObjectsCount, patientIdentifier, true);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(metadata.AsRegistryObjectDtos().ToList());

        var documents = metadata.Select(dto => dto.Document);

        foreach (var document in documents)
        {
            _repository.Write(document.DocumentId, document.Data, patientIdentifier);
        }

        return metadata;
    }

    private async Task NukeRegistryRepository()
    {
        var getNukeKey = await _client.GetAsync("api/get-nuke-key");

        var nukeResponse = JsonDocument.Parse(await getNukeKey.Content.ReadAsStringAsync());
        var nukeKey = nukeResponse.RootElement.GetProperty("nukeKey").GetString();

        var nuked = await _client.DeleteAsync($"/api/nuke?nukeKey={nukeKey}");

        Assert.Empty(_registry.ReadRegistry());
    }
}
#pragma warning restore CS8604, CS8602 // Possible null reference argument.
