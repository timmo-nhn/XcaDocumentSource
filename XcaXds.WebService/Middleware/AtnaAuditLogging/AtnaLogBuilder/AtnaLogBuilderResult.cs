using Hl7.Fhir.Model;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

namespace XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilder;

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
    public IPolicyInputStrategy? Strategy { get; init; }

    public static AtnaLogBuilderResult Fail(string message)
    {
        return new AtnaLogBuilderResult(message, false);
    }
}
