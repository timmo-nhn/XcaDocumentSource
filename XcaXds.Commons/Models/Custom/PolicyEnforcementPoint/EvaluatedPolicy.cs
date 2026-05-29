using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services;

public class EvaluatedPolicy
{
    public EvaluatedPolicy()
    {
        Conditions ??= [];
    }
    
    public string PolicyId { get; set; }
    public List<ConditionResult> Conditions { get; set; }
    public Decision Decision { get; set; }
    public string Reason { get; set; }
}