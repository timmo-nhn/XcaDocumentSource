using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Source.Implementations.AtnaLogDLQ.FileBased;

public class FileBasedAtnaLogDLQStore : IAtnaLogDLQStore
{
    private readonly ILogger<FileBasedAtnaLogDLQStore> _logger;

    private readonly string _repositoryPath;
    private readonly object _lock = new();

    public FileBasedAtnaLogDLQStore(ILogger<FileBasedAtnaLogDLQStore> logger)
    {
        _logger = logger;

        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("REPOSITORY_FILE_PATH");

        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _repositoryPath = Path.Combine(customPath);
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _repositoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "AtnaLogDLQ");
            Directory.CreateDirectory(_repositoryPath);
        }
    }

    public void DeleteLatestEvent()
    {
        var firstFile = Directory.EnumerateFiles(_repositoryPath).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstFile)) return;

        File.Delete(firstFile);
    }

    public AuditEvent? GetLatestEvent()
    {
        var serializer = new FhirJsonDeserializer();

        var firstFile = Directory.EnumerateFiles(_repositoryPath).FirstOrDefault(f => Path.GetFileName(f) != ".gitkeep");
        if (string.IsNullOrWhiteSpace(firstFile)) return null;

        var firstFileContent = File.ReadAllText(firstFile);

        return serializer.Deserialize<AuditEvent>(firstFileContent);
    }

    public OperationResponse StoreAuditEvent(AuditEvent auditEvent)
    {
        var fileName = auditEvent.Id + "__" + DateTime.UtcNow.ToString("ddMMyyHHmmss");
        var serializer = new FhirJsonSerializer();

        using var file = new FileStream(Path.Combine(_repositoryPath, fileName), FileMode.Create);

        var auditEventString = serializer.SerializeToBytes(auditEvent);
        file.Write(auditEventString);

        return OperationResponse.Success("AuditEvent stored successfully");
    }
}
