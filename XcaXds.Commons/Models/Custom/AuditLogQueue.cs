using Hl7.Fhir.Model;
using System.Threading.Channels;

namespace XcaXds.Commons.Models.Custom;

public interface IAuditLogQueue
{
    void Enqueue(Func<AuditEvent> auditEvent);
    public IAsyncEnumerable<Func<AuditEvent>> DequeueAllAsync(CancellationToken ct);
}

public class AuditLogQueue : IAuditLogQueue
{
    private readonly Channel<Func<AuditEvent>> _queue = Channel.CreateUnbounded<Func<AuditEvent>>();

    public void Enqueue(Func<AuditEvent> auditEvent)
    {
        _queue.Writer.TryWrite(auditEvent);
    }

    public async ValueTask EnqueueAsync(Func<AuditEvent> auditEvent, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(auditEvent, ct);
    }

    public IAsyncEnumerable<Func<AuditEvent>> DequeueAllAsync(CancellationToken ct) =>
        _queue.Reader.ReadAllAsync(ct);
}
