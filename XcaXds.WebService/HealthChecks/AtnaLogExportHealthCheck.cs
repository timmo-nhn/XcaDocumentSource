using Microsoft.Extensions.Diagnostics.HealthChecks;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.HealthChecks;

public class AtnaLogExportHealthCheck : IHealthCheck
{
    private static readonly TimeSpan ExportGracePeriod = TimeSpan.FromMinutes(5);

    private readonly MonitoringStatusService _monitoringStatusService;

    public AtnaLogExportHealthCheck(MonitoringStatusService monitoringStatusService)
    {
        _monitoringStatusService = monitoringStatusService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lastRequest = _monitoringStatusService.LastRequest;
        var lastAtnaLogExported = _monitoringStatusService.LastAtnaLogExported;

        var data = new Dictionary<string, object>
        {
            ["LastRequest"] = lastRequest,
            ["LastAtnaLogExported"] = lastAtnaLogExported
        };

        if (lastRequest == default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("No requests handled yet, nothing to export", data));
        }

        if (lastAtnaLogExported >= lastRequest)
        {
            return Task.FromResult(HealthCheckResult.Healthy("ATNA logs are exported up to the last handled request", data));
        }

        var timeSinceLastRequest = DateTimeOffset.UtcNow - lastRequest;

        if (timeSinceLastRequest <= ExportGracePeriod)
        {
            return Task.FromResult(HealthCheckResult.Healthy("ATNA log export is still within the grace period", data));
        }

        return Task.FromResult(new HealthCheckResult(
            context.Registration.FailureStatus,
            $"No ATNA log has been exported since the last request {timeSinceLastRequest.TotalMinutes:F0} minutes ago",
            data: data));
    }
}
