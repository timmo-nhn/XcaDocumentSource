using Abc.Xacml.Context;
using XcaXds.Commons.Commons;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint;

public class PolicyEvaluator
{
    private readonly PolicyDecisionPointService _pdp;

    public PolicyEvaluator(PolicyDecisionPointService pdp)
    {
        _pdp = pdp;
    }

    public PolicyDecisionResult Evaluate(XacmlContextRequest? req, Issuer appliesTo)
    {
        if (req == null)
        {
            return new PolicyDecisionResult(false, null);
        }

        var resp = _pdp.EvaluateXacmlRequest(req, appliesTo);
        var permit = resp.Results.All(r => r.Decision == XacmlContextDecision.Permit);

        return new PolicyDecisionResult(permit, resp);
    }
}
