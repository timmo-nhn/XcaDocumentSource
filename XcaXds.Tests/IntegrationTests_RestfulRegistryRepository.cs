using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

public partial class IntegrationTests_RestfulRegistryRepository_CRUD : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly InMemoryPolicyRepository _policyRepository;
    private readonly InMemoryRegistry _registry;
    private readonly InMemoryRepository _repository;
    private readonly ITestOutputHelper _output;

    private List<DocumentReferenceDto> RegistryContent { get; set; }

    private readonly int RegistryItemCount = 1000; // The amount of registry objects to generate and evaluate against

    private readonly CX PatientIdentifier = new()
    {
        IdNumber = "13116900216",
        AssigningAuthority = new HD()
        {
            UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
            UniversalId = Constants.Oid.Fnr
        }
    };

    public IntegrationTests_RestfulRegistryRepository_CRUD(
        WebApplicationFactory<WebService.Program> factory,
        ITestOutputHelper output)
    {
        _output = output;

        using var scope = factory.Services.CreateScope();

        _policyRepository = new InMemoryPolicyRepository();

        _registry = new InMemoryRegistry();
        _repository = new InMemoryRepository();

        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppStartupService>();
                services.RemoveAll<AtnaLogExporterService>();

                // Remove implementations defined in Program.cs (WebApplicationFactory<WebService.Program>) ...
                services.RemoveAll<IPolicyRepository>();
                services.RemoveAll<IRegistry>();
                services.RemoveAll<IRepository>();

                // ...so replace with the mock implementations
                services.AddSingleton<IPolicyRepository>(_policyRepository);
                services.AddSingleton<IRegistry>(_registry);
                services.AddSingleton<IRepository>(_repository);
            });
        });

        _client = customFactory.CreateClient();
        using var customScope = customFactory.Services.CreateScope();

        _restfulRegistryService = customScope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
        _policyRepositoryService = customScope.ServiceProvider.GetRequiredService<PolicyRepositoryService>();
        _registryWrapper = customScope.ServiceProvider.GetRequiredService<RegistryWrapper>();
    }


    [Fact]
    [Trait("Delete", "Registry/Repository")]
    public async Task Delete_OlderThanXMonths()
    {
        var days = Random.Shared.Next(30, 365);

        var documentEntries = EnsureRegistryAndRepositoryHasContent(patientIdentifier: PatientIdentifier.IdNumber).AsRegistryObjectList().OfType<DocumentEntryDto>().ToArray();

        var oldDocumentEntries = documentEntries.Where(de => de.ServiceStopTime < DateTime.Now.AddDays(-days)).ToArray();

        var url = QueryHelpers.AddQueryString("/api/rest/delete-older-than", "days", string.Empty + days); // Geeked up implicit type cast!?
        var firstResponse = await _client.DeleteAsync(url);

        Assert.Equal(_registry.DocumentRegistry.OfType<DocumentEntryDto>().Count(), documentEntries.Length - oldDocumentEntries.Length);
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