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
    public AuditLogExporterService(ILogger<AuditLogExporterService> logger, ApplicationConfig appConfig, IAuditLogQueue auditLogQueue)
    {
        _logger = logger;
        _appConfig = appConfig;
        _auditLogQueue = auditLogQueue;
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
        var serializer = new FhirJsonSerializer(new() { Pretty = true });
        var atnaJson = serializer.SerializeToString(auditEvent);
        _logger.LogDebug("Created FHIR AuditEvent: \n" + atnaJson);
        //File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "AuditEvents", $"{auditEvent.Id}.json"), atnaJson);
    }
}