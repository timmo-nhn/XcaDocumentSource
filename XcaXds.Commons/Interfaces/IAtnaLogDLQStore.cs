using Hl7.Fhir.Model;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Commons.Interfaces;

/// <summary>
/// Dead Letter Queue (DLQ) store interface for ATNA log events. 
/// This interface defines methods for storing and retrieving audit events that could not be exported successfully, 
/// allowing for later inspection or reprocessing.
/// </summary>
public interface IAtnaLogDLQStore
{
    OperationResponse StoreAuditEvent(AuditEvent auditEvent);
    AuditEvent? GetLatestEvent();
    void DeleteLatestEvent();
}