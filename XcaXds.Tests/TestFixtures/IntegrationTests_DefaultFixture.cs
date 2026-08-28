using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Extensions.NinParsers;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.ApiKey;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Shared;
using XcaXds.Terminology.Services;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.AtnaAuditLogging;
using XcaXds.WebService.Services.Fhir;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Startup;

namespace XcaXds.Tests;

#pragma warning disable CS8602, CS8604 // Dereference of a possibly null reference.
public class IntegrationTests_DefaultFixture : IAsyncDisposable
{
    // Keep a strong reference to the WebApplicationFactory created via WithWebHostBuilder.
    // Without this, it can be GC-collected (it has a finalizer) and dispose its ServiceProvider
    // while tests are still running, causing intermittent ObjectDisposedException (IServiceProvider)
    // when EF Core tries to resolve services.
    private readonly WebApplicationFactory<Program> _factory;

    internal readonly ApiKeyHolder _apiKeyHolder;
    internal readonly ApplicationMetaService _applicationMetaService;
    internal readonly ApplicationConfig _applicationConfig;
    internal readonly HttpClient _client;
    internal readonly RestfulRegistryRepositoryService _restfulRegistryService;
    internal readonly PolicyRepositoryService _policyRepositoryService;
    internal readonly PolicyDecisionPointService _policyDecisionPointService;
    internal readonly RegistryWrapper _registryWrapper;
    internal readonly IRegistry _registry;
    internal readonly IRepository _repository;
    internal readonly AtnaLogExportedChecker _atnaLogExportedChecker;
    internal readonly ITestOutputHelper _output;
    internal readonly DocumentListFiltererService _documentListFiltererService;
    internal readonly BusinessLogicFiltersRegistry _businessLogicFiltersRegistry;
    internal readonly BusinessRulesDescriptorService _businessRulesDescriptorService;
    internal readonly TerminologyService _terminologyService;
    internal readonly XdsOnFhirTransformerService _xdsOnFhirTransformerService;
    internal readonly FhirResourceValidatorService _fhirResourceValidatorService;
    internal readonly BusinessLogicMapperService _businessLogicMapperService;
    internal readonly DocumentObfuscationService _documentObfuscationService;
    internal readonly NinParserFactory _ninParserFactory;

    internal readonly IServiceScope _scope;

    public Uri BaseAddress { get; private set; } = default!;

    internal List<DocumentReferenceDto> RegistryContent { get; set; } = new();

    internal int RegistryItemCount = 100; // The amount of registry objects to generate and evaluate against

    internal readonly CX PatientIdentifier = new()
    {
        IdNumber = "17855599120",
        AssigningAuthority = new HD()
        {
            UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
            UniversalId = "2.16.578.1.12.4.1.4.1"
        }
    };

    public IntegrationTests_DefaultFixture(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;

        // Custom factory with fake services
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseTestServer", "false");
            builder.UseKestrel();
            builder.UseUrls("https://localhost:0");

            builder.ConfigureServices(services =>
            {
                // Test stability: background services run concurrently with the test.
                // If a BackgroundService throws, the default behavior is to stop the host,
                // which disposes the root IServiceProvider and can surface later as
                // ObjectDisposedException during EF Core queries.
                services.Configure<HostOptions>(o => { o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore; });

                services.RemoveAll<IHostedService>();
                services.AddHostedService<NonRequestingAtnaLogExporter>();
                services.AddHostedService<IntegrationTestCleanupService>();
                services.AddHostedService<MockStatisticsProcessorService>();
                services.AddHostedService<AppStartupService>();

                services.RemoveAll<IVirusScanner>();
                services.AddSingleton<IVirusScanner, FakeClamAvFileScanner>();
                services.AddSingleton<AtnaLogExportedChecker>();

                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                });
            });
        });

        // Force app to start
        _client = _factory.CreateDefaultClient();

        _scope = _factory.Services.CreateScope();

        _registry = _scope.ServiceProvider.GetRequiredService<IRegistry>();
        _repository = _scope.ServiceProvider.GetRequiredService<IRepository>();

        _documentObfuscationService = _scope.ServiceProvider.GetRequiredService<DocumentObfuscationService>();
        _applicationConfig = _scope.ServiceProvider.GetRequiredService<ApplicationConfig>();
        _terminologyService = _scope.ServiceProvider.GetRequiredService<TerminologyService>();
        _fhirResourceValidatorService = _scope.ServiceProvider.GetRequiredService<FhirResourceValidatorService>();
        _xdsOnFhirTransformerService = _scope.ServiceProvider.GetRequiredService<XdsOnFhirTransformerService>();
        _businessRulesDescriptorService = _scope.ServiceProvider.GetRequiredService<BusinessRulesDescriptorService>();
        _businessLogicFiltersRegistry = _scope.ServiceProvider.GetRequiredService<BusinessLogicFiltersRegistry>();
        _documentListFiltererService = _scope.ServiceProvider.GetRequiredService<DocumentListFiltererService>();
        _atnaLogExportedChecker = _scope.ServiceProvider.GetRequiredService<AtnaLogExportedChecker>();
        _atnaLogExportedChecker = _scope.ServiceProvider.GetRequiredService<AtnaLogExportedChecker>();
        _restfulRegistryService = _scope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
        _policyRepositoryService = _scope.ServiceProvider.GetRequiredService<PolicyRepositoryService>();
        _policyDecisionPointService = _scope.ServiceProvider.GetRequiredService<PolicyDecisionPointService>();
        _registryWrapper = _scope.ServiceProvider.GetRequiredService<RegistryWrapper>();
        _apiKeyHolder = _scope.ServiceProvider.GetRequiredService<ApiKeyHolder>();
        _applicationMetaService = _scope.ServiceProvider.GetRequiredService<ApplicationMetaService>();
        _businessLogicMapperService = _scope.ServiceProvider.GetRequiredService<BusinessLogicMapperService>();
        _ninParserFactory = _scope.ServiceProvider.GetRequiredService<NinParserFactory>();

        _client.DefaultRequestHeaders.Add("X-API-Key", _apiKeyHolder.ApiKey);
    }

    public async ValueTask DisposeAsync()
    {
        // Ensure the scope is torn down before the factory, so any scoped disposables are cleaned up.
        _scope?.Dispose();

        _client?.Dispose();

        // WebApplicationFactory implements IAsyncDisposable; dispose it deterministically.
        if (_factory is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
            _factory?.Dispose();
    }

    internal async Task WaitForUserAccessEntryToBeExported()
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(10);
        while (string.IsNullOrWhiteSpace(MockStatisticsProcessorService.UserAccessEntryJson) && DateTime.UtcNow < timeoutAt)
        {
            await Task.Delay(50);
        }

        Assert.False(string.IsNullOrWhiteSpace(MockStatisticsProcessorService.UserAccessEntryJson));
    }

    internal async Task WaitForAtnaLogToBeExported()
    {
        // Audit log is generated via background service; allow a brief window for the queue to be processed.
        var timeoutAt = DateTime.UtcNow.AddSeconds(10);
        while ((!_atnaLogExportedChecker.AtnaLogExported ||
                string.IsNullOrWhiteSpace(MockStatisticsProcessorService.UserAccessEntryJson)) &&
               DateTime.UtcNow < timeoutAt)
        {
            await Task.Delay(50);
        }

        Assert.True(_atnaLogExportedChecker.AtnaLogExported);
        Assert.False(string.IsNullOrWhiteSpace(MockStatisticsProcessorService.UserAccessEntryJson));
    }

    internal async Task<List<DocumentReferenceDto>> EnsureRegistryAndRepositoryHasContent(int registryObjectsCount = 10,
        string? patientIdentifier = null)
    {
        await NukeRegistryRepository();

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(registryObjectsCount, patientIdentifier, true);
        _registryWrapper.AddDocumentReferenceDtosToDocumentRegistry(metadata.AsRegistryObjectDtos().ToList());

        foreach (var document in metadata.Select(dto => dto.Document))
        {
            _repository.Write(document.DocumentId, document.Data, "gubbe");
        }

        return metadata;
    }

    internal async Task NukeRegistryRepository()
    {
        var applicationMetaService = _scope.ServiceProvider.GetRequiredService<ApplicationMetaService>();

        var registry = _scope.ServiceProvider.GetRequiredService<IRegistry>();

        var nukeKey = applicationMetaService.GetNukeKeyForRegistryRepository();

        applicationMetaService.NukeRegistryRepository(nukeKey);

        Assert.Empty(await registry.ReadRegistry().ToListAsync());
    }

    internal string GetTestDataFile(string v)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataDirectories = Directory.GetDirectories(testDataPath);
        var testDataFiles = Directory.GetFiles(testDataPath);

        var allFiles = testDataDirectories.SelectMany(Directory.GetFiles).ToList().Concat(testDataFiles);

        var file = File.ReadAllText(allFiles.FirstOrDefault(f => f.Contains(v))!);
        return file;
    }
}
#pragma warning restore CS8602, CS8604 // Dereference of a possibly null reference.