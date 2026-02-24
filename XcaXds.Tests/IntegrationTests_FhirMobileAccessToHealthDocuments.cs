using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

public class IntegrationTests_FhirMobileAccessToHealthDocuments : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_FhirMobileAccessToHealthDocuments(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata()
    {
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
    [Trait("Upload", "Provide Bundle")]
    public async Task ProvideBundle_RandomAmount()
    {
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

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,"/R4/fhir/Bundle");
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
