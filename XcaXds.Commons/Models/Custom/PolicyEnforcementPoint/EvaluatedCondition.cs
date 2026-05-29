using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services;

public class EvaluatedCondition
{
    public EvaluatedCondition()
    {
    }

    public EvaluatedCondition(bool matches, List<ConditionResult> diagnostics)
    {
        Matches = matches;
        Diagnostics = diagnostics;
    }

    public bool Matches { get; set; }
    public List<ConditionResult> Diagnostics { get; set; }
}