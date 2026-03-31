using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace XcaXds.Commons.Models.Custom;

public interface IAtnaLogQueue
{
    void Enqueue(Func<AuditEvent> auditEvent);
    public IAsyncEnumerable<Func<AuditEvent>> DequeueAllAsync(CancellationToken ct);
}

public class AtnaLogQueue : IAtnaLogQueue
{
    private readonly ILogger<AtnaLogQueue> _logger;
    private readonly Channel<Func<AuditEvent>> _queue = Channel.CreateUnbounded<Func<AuditEvent>>();
    public AtnaLogQueue(ILogger<AtnaLogQueue> logger)
    {
        _logger = logger;
    }

    public void Enqueue(Func<AuditEvent> auditEvent)
    {
        try
        {
            _queue.Writer.TryWrite(auditEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auditlog");
        }
    }

    public async ValueTask EnqueueAsync(Func<AuditEvent> auditEvent, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(auditEvent, ct);
    }

    public IAsyncEnumerable<Func<AuditEvent>> DequeueAllAsync(CancellationToken ct) =>
        _queue.Reader.ReadAllAsync(ct);
}
