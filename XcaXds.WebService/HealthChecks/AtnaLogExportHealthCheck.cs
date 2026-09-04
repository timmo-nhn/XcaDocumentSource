using Microsoft.Extensions.Diagnostics.HealthChecks;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.AtnaAuditLogging;

namespace XcaXds.WebService.HealthChecks;

public class AtnaLogExportHealthCheck : IHealthCheck
{
    private static readonly TimeSpan ExportGracePeriod = TimeSpan.FromSeconds(15);

    private readonly MonitoringStatusService _monitoringStatusService;
    private readonly ApplicationMetaService _applicationMetaService;

    public AtnaLogExportHealthCheck(MonitoringStatusService monitoringStatusService, ApplicationMetaService applicationMetaService)
    {
        _monitoringStatusService = monitoringStatusService;
        _applicationMetaService = applicationMetaService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lastRequest = _monitoringStatusService.LastAtnalogEligibleRequest;
        var lastAtnaLogExported = _monitoringStatusService.LastAtnaLogExported;

        var data = new Dictionary<string, object>
        {
            ["LastRequest"] = lastRequest,
            ["LastAtnaLogExported"] = lastAtnaLogExported
        };

        var atnaLogExporterHealth = await _applicationMetaService.AtnaLogHealthCheck();

        data["AtnaLogExporter"] = new
        {
            atnaLogExporterHealth.Status,
            atnaLogExporterHealth.Description,
            Exception = atnaLogExporterHealth.Exception?.Message,
            atnaLogExporterHealth.Data
        };

        if (atnaLogExporterHealth.Status != HealthStatus.Healthy)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                $"Error from AtnalogExporter HealthCheck-endpoint. Configured Endpoint: {AtnaLogExporterService.AtnaLogEndpointUrl}. Health endpoint: {AtnaLogExporterService.AtnaLogHealthUrl}",
                data: data);
        }

        if (lastRequest == default)
        {
            return HealthCheckResult.Healthy("No requests handled yet, nothing to export", data);
        }

        if (lastAtnaLogExported >= lastRequest)
        {
            return HealthCheckResult.Healthy("ATNA logs are exported up to the last handled request", data);
        }

        var timeSinceLastRequest = DateTimeOffset.UtcNow - lastRequest;

        if (timeSinceLastRequest <= ExportGracePeriod)
        {
            return HealthCheckResult.Healthy("ATNA log export is still within the grace period", data);
        }

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            $"No ATNA log has been exported since the last request {timeSinceLastRequest.TotalMinutes:F0} minutes ago ({timeSinceLastRequest.TotalSeconds:F0} seconds)",
            data: data);
    }
}
