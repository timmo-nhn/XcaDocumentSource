using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;
using Xunit.Abstractions;


namespace XcaXds.Tests;

#pragma warning disable CS8602, CS8604 // Dereference of a possibly null reference.
public class IntegrationTests_DefaultFixture
{
    internal readonly HttpClient _client;
    internal readonly RestfulRegistryRepositoryService _restfulRegistryService;
    internal readonly PolicyRepositoryService _policyRepositoryService;
    internal readonly RegistryWrapper _registryWrapper;
    internal readonly IRegistry _registry;
    internal readonly IRepository _repository;
    internal readonly AtnaLogExportedChecker _atnaLogExportedChecker;
    internal readonly ITestOutputHelper _output;
    public Uri BaseAddress { get; private set; } = default!;

    internal List<DocumentReferenceDto> RegistryContent { get; set; } = new();

    internal int RegistryItemCount = 100; // The amount of registry objects to generate and evaluate against

    internal readonly CX PatientIdentifier = new()
    {
        IdNumber = "17855599120",
        AssigningAuthority = new HD()
        {
            UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
            UniversalId = Constants.Oid.Fnr
        }
    };

    public IntegrationTests_DefaultFixture(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output)
    {
        _output = output;

        // Custom factory with fake services
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseTestServer", "false");
            builder.UseKestrel();
            builder.UseUrls("https://localhost:0");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppStartupService>();

                //// Remove implementations defined in Program.cs (WebApplicationFactory<WebService.Program>) ...
                //services.RemoveAll<IRepository>();
                //services.RemoveAll<IPolicyRepository>();
                //services.RemoveAll<IRegistry>();
                //// ...so replace with the mock implementations
                //services.AddSingleton<IRepository>(new InMemoryRepository());
                //services.AddSingleton<IPolicyRepository>(new InMemoryPolicyRepository());
                //services.AddSingleton<IRegistry>(new InMemoryRegistry());

                services.RemoveAll<AtnaLogExporterService>();
                services.RemoveAll<IHostedService>();
                services.RemoveAll<IClamAvFileScanner>();

                services.AddSingleton<IClamAvFileScanner, FakeClamAvFileScanner>();
                services.AddSingleton<AtnaLogExportedChecker>();
                services.AddHostedService<NonRequestingAtnaLogExporter>();
                services.AddHostedService<IntegrationTestCleanupService>();

                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });
        });

        // Force app to start
        _client = customFactory.CreateDefaultClient();

        using var customScope = customFactory.Services.CreateScope();

        _registry = customScope.ServiceProvider.GetRequiredService<IRegistry>();
        _repository = customScope.ServiceProvider.GetRequiredService<IRepository>();

        _atnaLogExportedChecker = customScope.ServiceProvider.GetRequiredService<AtnaLogExportedChecker>();
        _restfulRegistryService = customScope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
        _policyRepositoryService = customScope.ServiceProvider.GetRequiredService<PolicyRepositoryService>();
        _registryWrapper = customScope.ServiceProvider.GetRequiredService<RegistryWrapper>();
    }

    internal async Task WaitForAtnaLogToBeExported()
    {
        // Audit is generated via background service; allow a brief window for the queue to be drained.
        var timeoutAt = DateTime.UtcNow.AddSeconds(4);
        while (!_atnaLogExportedChecker.AtnaLogExported && DateTime.UtcNow < timeoutAt)
        {
            await Task.Delay(50);
        }

        Assert.True(_atnaLogExportedChecker.AtnaLogExported);
    }

    internal async Task<List<DocumentReferenceDto>> EnsureRegistryAndRepositoryHasContent(int registryObjectsCount = 10, string? patientIdentifier = null)
    {
        await NukeRegistryRepository();

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(registryObjectsCount, patientIdentifier, true);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(metadata.AsRegistryObjectDtos().ToList());

        foreach (var document in metadata.Select(dto => dto.Document))
        {
            _repository.Write(document.DocumentId, document.Data, "gubbe");
        }

        return metadata;
    }

    internal async Task NukeRegistryRepository()
    {
        var getNukeKey = await _client.GetAsync("api/get-nuke-key");
        var stringContent = await getNukeKey.Content.ReadAsStringAsync();
        var nukeResponse = JsonDocument.Parse(stringContent);
        var nukeKey = nukeResponse.RootElement.GetProperty("nukeKey").GetString();

        var nuked = await _client.DeleteAsync($"/api/nuke?nukeKey={nukeKey}");

        Assert.Empty(_registry.ReadRegistry());
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