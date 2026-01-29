using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Commons.Models.Custom;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.Services;

public class AtnaLogExporterService : BackgroundService
{
    private readonly ILogger<AtnaLogExporterService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAtnaLogQueue _atnaLogQueue;
    private readonly IHttpClientFactory _httpClientFactory;

    private string _auditEventPath;


    public AtnaLogExporterService(ILogger<AtnaLogExporterService> logger, ApplicationConfig appConfig, IAtnaLogQueue atnaLogQueue, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _appConfig = appConfig;
        _atnaLogQueue = atnaLogQueue;
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
        try
        {
            await foreach (var auditEventFunction in _atnaLogQueue.DequeueAllAsync(stoppingToken).WithCancellation(stoppingToken))
            {
                var auditEvent = auditEventFunction();
                ExportAtnaLog(auditEvent);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("AuditLogExporterService is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in AuditLogExporterService");
            throw;
        }
    }

    private void ExportAtnaLog(AuditEvent auditEvent)
    {
        var serializer = new FhirJsonSerializer();
        var atnaJson = serializer.SerializeToString(auditEvent,true);
        _logger.LogDebug("Created FHIR AuditEvent: \n" + atnaJson);
        //File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "AuditEvents", $"{auditEvent.Id}.json"), atnaJson);
    }
}