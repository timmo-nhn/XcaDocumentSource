using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Commons.Models.Custom;
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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var auditEventFunction in _atnaLogQueue.DequeueAllAsync(stoppingToken).WithCancellation(stoppingToken))
        {
            _lastAuditEvent = auditEventFunction();
            await ExportAuditEvent(_lastAuditEvent, stoppingToken, false);
        }
    }

    private async Task<bool> ExportAuditEvent(AuditEvent auditEvent, CancellationToken stoppingToken, bool fromDlq)
    {
        try
        {
            bool result = false;
            ExecuteWithRetry(async () =>
            {
                var serializer = new FhirJsonSerializer();
                var auditEventJson = serializer.SerializeToString(auditEvent, true);
                _logger.LogDebug("Created FHIR AuditEvent: \n" + auditEventJson);

                var client = _httpClientFactory.CreateClient();

                var endpointUrl = $"{_appConfig.AtnaLogExporterEndpoint}";

                var response = await client.PostAsync(endpointUrl, new StringContent(auditEventJson, System.Text.Encoding.UTF8, "application/fhir+json"), CancellationToken.None);

                var responseBody = await response.Content.ReadAsStringAsync(CancellationToken.None);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully exported AuditEvent {auditEventId} to {atnaLogExporterEndpoint}", auditEvent.Id, _appConfig.AtnaLogExporterEndpoint);
                    _monitoringStatusService.LastAtnaLogExported = DateTimeOffset.UtcNow;

                    await HandleDlq(auditEvent, fromDlq);

                    result = true;
                }
                else
                {
                    var deserializer = new FhirJsonDeserializer();
                    var operationOutcome = deserializer.Deserialize<OperationOutcome>(responseBody);

                    _logger.LogError("Failed to export AuditEvent {auditEventId} to {atnaLogExporterEndpoint}. Status Code: {statusCode}, Response: {issues}", auditEvent.Id, _appConfig.AtnaLogExporterEndpoint, response.StatusCode, string.Join(", ", operationOutcome.Issue.Select(iss => iss.Severity + " " + iss.Details?.Text)));
                }
            });

            return result;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("AuditLogExporterService is stopping.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation(ex.ToString());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to connect to ATNA log exporter endpoint {atnaLogExporterEndpoint}. Storing in DLQ", _appConfig.AtnaLogExporterEndpoint);
            if (_lastAuditEvent != null)
            {
                var response = _auditLogDLQService.StoreAuditEvent(_lastAuditEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in AuditLogExporterService. Audit log is not being exported!");
        }

        return false;
    }

    private void ExecuteWithRetry(Action action, int retries = 3)
    {
        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                _logger.LogInformation("RetryLogic Attempt {attempt}/{maxAttempts}", attempt, retries);
                action();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit event export failed on attempt {attempt}/{maxAttempts}", attempt, retries);
                if (attempt == retries) return;
                Thread.Sleep(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
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
                var exportSuccess = await ExportAuditEvent(dlqEvent, CancellationToken.None, true);
                if (exportSuccess) _auditLogDLQService.DeleteLatestEvent();
            }
        }
    }
}