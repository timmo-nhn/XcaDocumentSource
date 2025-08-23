using Abc.Xacml.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class PolicyDecisionPointService
{
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyDecisionPointService(PolicyRepositoryWrapper policyRepositoryWrapper)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
    }
    public XacmlContextResponse EvaluateRequest(XacmlContextRequest xacmlRequest)
    {
        return _policyRepositoryWrapper.EvaluateRequest_V20(xacmlRequest);
    }
}
