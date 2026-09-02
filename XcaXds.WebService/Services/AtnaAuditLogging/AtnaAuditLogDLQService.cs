using Hl7.Fhir.Model;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.WebService.Services.AtnaAuditLogging;

public class AtnaAuditLogDLQService
{
    private readonly ILogger<AtnaAuditLogDLQService> _logger;
    private readonly IAtnaLogDLQStore _atnaLogDLQStore;
 
    private bool _hasItemsInQueue = false;
    private int _dlqCount = 0;

    public AtnaAuditLogDLQService(ILogger<AtnaAuditLogDLQService> logger, IAtnaLogDLQStore atnaLogDLQStore)
    {
        _logger = logger;
        _atnaLogDLQStore = atnaLogDLQStore;

        // Get latest event to bump the state of _hasItemsInQueue variable
        GetLatestEvent();
    }

    public OperationResponse StoreAuditEvent(AuditEvent auditEvent)
    {
        _dlqCount++;
        _hasItemsInQueue = true;
        return _atnaLogDLQStore.StoreAuditEvent(auditEvent);
    }

    public AuditEvent? GetLatestEvent()
    {
        var auditEvent = _atnaLogDLQStore.GetLatestEvent();
        _hasItemsInQueue = true;
        if (auditEvent == null)
        {
            _hasItemsInQueue = false;
        }
        return auditEvent;
    }

    public void DeleteLatestEvent()
    {
        _dlqCount--;
        _atnaLogDLQStore.DeleteLatestEvent();
    }

    public bool HasItemsInQueue()
    {
        return _hasItemsInQueue;
    }

    public int GetDlqItemCount()
    {
        return _dlqCount;
    }
}
