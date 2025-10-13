using Abc.Xacml.Context;
using XcaXds.Commons.Commons;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class PolicyDecisionPointService
{
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyDecisionPointService(PolicyRepositoryWrapper policyRepositoryWrapper)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
    }

    public XacmlContextResponse EvaluateRequest(XacmlContextRequest? xacmlRequest, Issuer xacmlRequestAppliesTo)
    {
        return _policyRepositoryWrapper.EvaluateRequest_V20(xacmlRequest);
    }
}
