using Abc.Xacml.Context;
using XcaXds.Commons.Commons;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint;

public class PolicyInputResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public XacmlContextRequest? XacmlRequest { get; init; }
    public Issuer AppliesTo { get; init; } = Issuer.Unknown;

    public bool IsSoap { get; init; }
    public bool IsFhir { get; init; }
    public bool IsJson { get; init; }
}
