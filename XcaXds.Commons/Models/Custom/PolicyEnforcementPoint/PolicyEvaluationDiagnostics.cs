using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services;

public class PolicyEvaluationDiagnostics
{
    public string? Id { get; set; }
    public Decision Decision { get; set; }
    public List<ConditionResult>? FailedConditions { get; set; }
    public List<ConditionResult>? MatchedConditions { get; set; }
}