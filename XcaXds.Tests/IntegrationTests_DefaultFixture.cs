using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;
using Xunit.Abstractions;

namespace XcaXds.Tests;

public class IntegrationTests_DefaultFixture
{
    internal readonly HttpClient _client;
    internal readonly RestfulRegistryRepositoryService _restfulRegistryService;
    internal readonly PolicyRepositoryService _policyRepositoryService;
    internal readonly RegistryWrapper _registryWrapper;
    internal readonly InMemoryPolicyRepository _policyRepository;
    internal readonly InMemoryRegistry _registry;
    internal readonly InMemoryRepository _repository;
    internal readonly AtnaLogExportedChecker _atnaLogExportedChecker;
    internal readonly ITestOutputHelper _output;

    internal List<DocumentReferenceDto> RegistryContent { get; set; } = new();

    internal readonly int RegistryItemCount = 500; // The amount of registry objects to generate and evaluate against

    internal readonly CX PatientIdentifier = new()
    {
        IdNumber = "13116900216",
        AssigningAuthority = new HD()
        {
            UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
            UniversalId = Constants.Oid.Fnr
        }
    };

    public IntegrationTests_DefaultFixture(
        WebApplicationFactory<WebService.Program> factory,
        ITestOutputHelper output)
    {
        _output = output;

        using var scope = factory.Services.CreateScope();

        _policyRepository = new InMemoryPolicyRepository();
        _registry = new InMemoryRegistry();
        _repository = new InMemoryRepository();

        // Custom factory with fake services
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppStartupService>();

                // Remove implementations defined in Program.cs (WebApplicationFactory<WebService.Program>) ...
                services.RemoveAll<IPolicyRepository>();
                services.RemoveAll<IRegistry>();
                services.RemoveAll<IRepository>();

                services.RemoveAll<AtnaLogExporterService>();
                services.RemoveAll<IHostedService>();

                // ...so replace with the mock implementations
                services.AddSingleton<IPolicyRepository>(_policyRepository);
                services.AddSingleton<IRegistry>(_registry);
                services.AddSingleton<IRepository>(_repository);
                services.AddSingleton<AtnaLogExportedChecker>();
                services.AddHostedService<NonRequestingAtnaLogExporter>();

                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });

            builder.UseEnvironment("Testing");
        });

        _client = customFactory.CreateClient();
        using var customScope = customFactory.Services.CreateScope();

        _atnaLogExportedChecker = customScope.ServiceProvider.GetRequiredService<AtnaLogExportedChecker>();
        _restfulRegistryService = customScope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
        _policyRepositoryService = customScope.ServiceProvider.GetRequiredService<PolicyRepositoryService>();
        _registryWrapper = customScope.ServiceProvider.GetRequiredService<RegistryWrapper>();
    }
}