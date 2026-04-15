using Abc.Xacml.Context;
using XcaXds.Commons.Commons;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint;

public class PolicyEvaluator
{
    private readonly PolicyDecisionPointService _pdp;

    public PolicyEvaluator(PolicyDecisionPointService pdp)
    {
        _pdp = pdp;
    }

    public PolicyDecisionResult Evaluate(XacmlContextRequest? req)
    {
        if (req == null)
        {
            return new PolicyDecisionResult(false, null);
        }

        var resp = _pdp.EvaluateXacmlRequest(req);
        var permit = resp.Results.All(r => r.Decision == XacmlContextDecision.Permit);

        return new PolicyDecisionResult(permit, resp);
    }
}
