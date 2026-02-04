using Abc.Xacml.Context;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint;

public class PolicyDecisionResult
{
    public bool Permit { get; set; }
    public XacmlContextResponse Response { get; set; }

    public PolicyDecisionResult(bool permit, XacmlContextResponse resp)
    {
        Permit = permit;
        Response = resp;
    }
}