using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services;

public class EvaluatedCondition
{
    public bool Matches { get; set; }
    public List<ConditionResult>? Diagnostics { get; set; }

    public EvaluatedCondition()
    {
    }

    public EvaluatedCondition(bool matches, List<ConditionResult> diagnostics)
    {
        Matches = matches;
        Diagnostics = diagnostics;
    }
}