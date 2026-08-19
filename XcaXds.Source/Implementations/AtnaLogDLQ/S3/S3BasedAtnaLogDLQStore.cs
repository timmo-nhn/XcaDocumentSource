using Hl7.Fhir.Model;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Source.Implementations.AtnaLogDLQ.S3;

public class S3BasedAtnaLogDLQStore : IAtnaLogDLQStore
{
    public void DeleteLatestEvent()
    {
        throw new NotImplementedException();
    }

    public AuditEvent[] GetAllEventsInQueue()
    {
        throw new NotImplementedException();
    }

    public AuditEvent? GetLatestEvent()
    {
        throw new NotImplementedException();
    }

    public OperationResponse StoreAuditEvent(AuditEvent auditEvent)
    {
        throw new NotImplementedException();
    }
}
