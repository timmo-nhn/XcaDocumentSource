using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.Metrics;
using System.Text.Json;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared.Extensions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.Services.AtnaAuditLogging;

public class AtnaLogExporterService : BackgroundService
{
    private readonly ILogger<AtnaLogExporterService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly MonitoringStatusService _monitoringStatusService;
    private readonly AtnaAuditLogDLQService _auditLogDLQService;
    private readonly IAtnaLogQueue _atnaLogQueue;
    private readonly IHttpClientFactory _httpClientFactory;

    private AuditEvent? _lastAuditEvent;
    private string _atnaLogBaseUrl;
    public static string? AtnaLogEndpointUrl;
    public static string? AtnaLogHealthUrl;

    private static readonly Meter _meter = new("XcaXds.AtnaLog");
    private static readonly ObservableGauge<int> _dlqGauge;

    public AtnaLogExporterService(
        ILogger<AtnaLogExporterService> logger,
        IAtnaLogQueue atnaLogQueue,
        IHttpClientFactory httpClientFactory,
        ApplicationConfig appConfig,
        MonitoringStatusService monitoringStatusService,
        AtnaAuditLogDLQService auditLogDLQService
        )
    {
        _logger = logger;
        _atnaLogQueue = atnaLogQueue;
        _httpClientFactory = httpClientFactory;
        _appConfig = appConfig;
        _monitoringStatusService = monitoringStatusService;
        _auditLogDLQService = auditLogDLQService;

        _atnaLogBaseUrl = StringExtensions.GetHostFromUrl(_appConfig.AtnaLogExporterEndpoint);
        AtnaLogEndpointUrl = $"{_atnaLogBaseUrl}/R4/fhir/AuditEvent";
        AtnaLogHealthUrl = $"{_atnaLogBaseUrl}/healthz";

        _meter.CreateObservableGauge("atna_dlq_count", () => _auditLogDLQService.GetDlqItemCount(), description: "Number of audit events currently in the dead letter queue");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var auditEventFunction in _atnaLogQueue.DequeueAllAsync(stoppingToken).WithCancellation(stoppingToken))
        {
            _lastAuditEvent = auditEventFunction();
            await ExportAuditEvent(_lastAuditEvent, false, stoppingToken);
        }
    }

    public async Task<HealthCheckResult> CheckHealth()
    {
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync(AtnaLogHealthUrl, CancellationToken.None);
        return JsonSerializer.Deserialize<HealthCheckResult>(await response.Content.ReadAsStringAsync());
    }

    public int GetDlqItems()
    {
        return _auditLogDLQService.GetDlqItemCount();
    }

    private async Task<bool> ExportAuditEvent(AuditEvent auditEvent, bool fromDlq, CancellationToken stoppingToken)
    {
        bool result = false;

        await ExecuteWithRetryAsync(async () =>
        {
            var serializer = new FhirJsonSerializer();
            var auditEventJson = serializer.SerializeToString(auditEvent, true);
            _logger.LogDebug("Created FHIR AuditEvent: \n" + auditEventJson);

            var client = _httpClientFactory.CreateClient();


            var response = await client.PostAsync(AtnaLogEndpointUrl, new StringContent(auditEventJson, System.Text.Encoding.UTF8, "application/fhir+json"), CancellationToken.None);

            var responseBody = await response.Content.ReadAsStringAsync(CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully exported AuditEvent {auditEventId} to {atnaLogExporterEndpoint}", auditEvent.Id, AtnaLogEndpointUrl);
                _monitoringStatusService.LastAtnaLogExported = DateTimeOffset.UtcNow;

                // If success, it means the atnalogexporter is up and running, which also means we are most likely good to handle the DLQ events
                // ExportAsync runs 
                await HandleDlq(auditEvent, fromDlq);

                result = true;
            }
            else
            {
                var deserializer = new FhirJsonDeserializer();
                var operationOutcome = deserializer.Deserialize<OperationOutcome>(responseBody);

                _logger.LogError("Failed to export AuditEvent {auditEventId} to {atnaLogExporterEndpoint}. Status Code: {statusCode}, Response: {issues}", auditEvent.Id, AtnaLogEndpointUrl, response.StatusCode, string.Join(", ", operationOutcome.Issue.Select(iss => iss.Severity + " " + iss.Details?.Text)));
            }
        }, stoppingToken);

        return result;
    }

    private async Task ExecuteWithRetryAsync(Func<Task> action, CancellationToken stoppingToken, int retries = 3)
    {
        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                _logger.LogInformation("RetryLogic Attempt {attempt}/{maxAttempts}", attempt, retries);
                await action();
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AuditLogExporterService is stopping.");
                return;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Audit event export failed on attempt {attempt}/{maxAttempts} (connection error)", attempt, retries);
                if (attempt == retries)
                {
                    _logger.LogError(ex, "Unable to connect to ATNA log exporter endpoint {atnaLogExporterEndpoint}. Storing in DLQ", AtnaLogEndpointUrl);
                    if (_lastAuditEvent != null)
                        _auditLogDLQService.StoreAuditEvent(_lastAuditEvent);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit event export failed on attempt {attempt}/{maxAttempts}", attempt, retries);
                if (attempt == retries)
                {
                    _logger.LogCritical(ex, "Unhandled exception in AuditLogExporterService. Audit log is not being exported! {dlqCount}", _auditLogDLQService.GetDlqItemCount());
                    if (_lastAuditEvent != null)
                        _auditLogDLQService.StoreAuditEvent(_lastAuditEvent);
                    return;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), CancellationToken.None);
        }
    }

    private async Task HandleDlq(AuditEvent auditEvent, bool fromDlq)
    {
        var processingQueue = _auditLogDLQService.HasItemsInQueue() && fromDlq == false;

        if (processingQueue)
        {
            _logger.LogInformation("There are items in the DLQ. Releasing events for export.");

            while (_auditLogDLQService.GetLatestEvent() is { } dlqEvent)
            {
                var exportSuccess = await ExportAuditEvent(dlqEvent, true, CancellationToken.None);
                if (exportSuccess) _auditLogDLQService.DeleteLatestEvent();
            }
        }
    }
}