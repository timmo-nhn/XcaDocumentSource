using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Commons.Models.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint;

public class AccessControlResponse
{
    public bool Permit => Decision == Decision.Permit;
    public AccessControlResult? Response { get; set; }
    public string PolicyId { get; set; }
    public List<PolicyEvaluationDiagnostics> Diagnostics { get; set; } = new();

    public string Reason { get; set; }
    public Decision Decision { get; set; }

    public AccessControlResponse()
    {
    }
}

public enum Decision
{
    NotApplicable,
    Deny,
    Permit,
}