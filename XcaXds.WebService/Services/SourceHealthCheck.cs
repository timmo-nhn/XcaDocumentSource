using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XcaXds.WebService.Services;

public class SourceHealthCheck : IHealthCheck
{
    private readonly SourceHealthCheckService _sourceHealthCheckService;

    public SourceHealthCheck(SourceHealthCheckService sourceHealthCheckService)
    {
        _sourceHealthCheckService = sourceHealthCheckService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var status = await _sourceHealthCheckService.GetRegistryRepositoryStatus();

        var data = new Dictionary<string, object>
        {
            ["RegistryReadOk"] = status.RegistryReadOk,
            ["RegistryWriteOk"] = status.RegistryWriteOk,
            ["RepositoryReadOk"] = status.RepositoryReadOk,
            ["RepositoryWriteOk"] = status.RepositoryWriteOk,
        };

        var allOk = status.RegistryReadOk && status.RegistryWriteOk
                    && status.RepositoryReadOk && status.RepositoryWriteOk;

        return allOk
            ? HealthCheckResult.Healthy("Registry and repository are operational.", data)
            : HealthCheckResult.Unhealthy("One or more registry/repository checks failed.", data: data);
    }
}
