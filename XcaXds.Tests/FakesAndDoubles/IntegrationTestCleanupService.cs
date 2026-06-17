using Microsoft.Extensions.Hosting;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.Policy;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests.FakesAndDoubles;

public class IntegrationTestCleanupService : IHostedService
{
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly HttpClient _httpClient;
    private readonly ApplicationMetaService _applicationMetaService;

    public IntegrationTestCleanupService(HttpClient httpClient, PolicyRepositoryService policyRepositoryService, ApplicationMetaService applicationMetaService)
    {
        _applicationMetaService = applicationMetaService;
        _policyRepositoryService = policyRepositoryService;
        _httpClient = httpClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _applicationMetaService.NukeRegistryRepository(_applicationMetaService.GetNukeKeyForRegistryRepository());
        _policyRepositoryService.DeleteAllPolicies();
    }
}