using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Commons.Models.Custom;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.Services;

public class AuditLogExporterService : BackgroundService
{
    private readonly ILogger<AuditLogExporterService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAuditLogQueue _auditLogQueue;
    private readonly IHttpClientFactory _httpClientFactory;

    private string _auditEventPath;

    public AuditLogExporterService(ILogger<AuditLogExporterService> logger, ApplicationConfig appConfig, IAuditLogQueue auditLogQueue, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _appConfig = appConfig;
        _auditLogQueue = auditLogQueue;
        _httpClientFactory = httpClientFactory;

        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("AUDITEVENTS_FILE_PATH");
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _auditEventPath = customPath;
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _auditEventPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "AuditEvents");

        }
        Directory.CreateDirectory(_auditEventPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var auditEventFunction in _auditLogQueue.DequeueAllAsync(stoppingToken))
        {
            var auditEvent = auditEventFunction();
            ExportAuditEvent(auditEvent);
        }
    }

    private void ExportAuditEvent(AuditEvent auditEvent)
    {
        var serializer = new FhirJsonSerializer();
        var atnaJson = serializer.SerializeToString(auditEvent,true);
        _logger.LogDebug("Created FHIR AuditEvent: \n" + atnaJson);
        //File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "AuditEvents", $"{auditEvent.Id}.json"), atnaJson);
    }
}