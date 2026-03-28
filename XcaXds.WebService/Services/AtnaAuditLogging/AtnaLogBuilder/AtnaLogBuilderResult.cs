using Hl7.Fhir.Model;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

public class AtnaLogBuilderResult
{
    public AtnaLogBuilderResult(string message, bool success = false)
    {
        Message = message;
        IsSuccess = success;
    }

    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public AuditEvent? AuditEvent { get; init; }
    public IAtnaLogStrategy? Strategy { get; init; }

    public static AtnaLogBuilderResult Fail(string message)
    {
        return new AtnaLogBuilderResult(message, false);
    }

    public static AtnaLogBuilderResult Success(string message)
    {
        return new AtnaLogBuilderResult(message, true);
    }

    public static AtnaLogBuilderResult Success(string message, IAtnaLogStrategy strategy)
    {
        return new AtnaLogBuilderResult(message, true)
        {
            Strategy = strategy
        };
    }
}
