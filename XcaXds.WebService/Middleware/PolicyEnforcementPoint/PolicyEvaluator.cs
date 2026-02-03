using Abc.Xacml.Context;
using XcaXds.Commons.Commons;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint;

public class PolicyEvaluator
{
    private readonly PolicyDecisionPointService _pdp;

    public PolicyEvaluator(PolicyDecisionPointService pdp)
    {
        _pdp = pdp;
    }

    public PolicyDecisionResult Evaluate(XacmlContextRequest req, Issuer appliesTo)
    {
        var resp = _pdp.EvaluateXacmlRequest(req, appliesTo);
        var permit = resp.Results.All(r => r.Decision == XacmlContextDecision.Permit);

        return new PolicyDecisionResult(permit, resp);
    }
}
