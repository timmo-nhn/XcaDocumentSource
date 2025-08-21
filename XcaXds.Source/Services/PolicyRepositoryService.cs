using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class PolicyRepositoryService
{
    private readonly ApplicationConfig _appConfig;
    private readonly ILogger<PolicyRepositoryService> _logger;
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyRepositoryService(PolicyRepositoryWrapper policyRepositoryWrapper, ILogger<PolicyRepositoryService> logger)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
        _logger = logger;
    }

    public PolicySetDto GetPoliciesAsPolicySetDto()
    {
        return _policyRepositoryWrapper.GetPolicies();
    }

    public bool AddPolicy(PolicyDto policyDto)
    {
        return _policyRepositoryWrapper.AddPolicy(policyDto);
    }

    public bool DeletePolicy(string id)
    {
        return _policyRepositoryWrapper.DeletePolicy(id);
    }

    public XacmlPolicySet GetPoliciesAsXacmlPolicySet()
    {
        var policySetDto = _policyRepositoryWrapper.GetPolicies();
        return PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySetDto);
    }

    public XacmlContextResponse EvaluateRequest(XacmlContextRequest xacmlRequest)
    {
        return _policyRepositoryWrapper.EvaluateRequest_V20(xacmlRequest);
    }
}
