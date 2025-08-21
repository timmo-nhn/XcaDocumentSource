using System.Xml;
using Abc.Xacml.Context;
using Abc.Xacml.Runtime;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Services;

namespace XcaXds.Source.Source;

public class PolicyRepositoryWrapper
{
    internal volatile PolicySetDto _policySet;

    internal volatile EvaluationEngine _evaluationEngine;
    
    private readonly IPolicyRepository _policyRepository;

    private readonly ILogger<PolicyRepositoryWrapper> _logger;


    public PolicyRepositoryWrapper(IPolicyRepository policyRepository, ILogger<PolicyRepositoryWrapper> logger)
    {
        _logger = logger;
        _policyRepository = policyRepository;
        _policySet ??= _policyRepository.GetAllPolicies();
        _evaluationEngine = new EvaluationEngine(PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(_policySet));
    }

    // For use in unit tests
    public PolicyRepositoryWrapper(FileBasedPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
        _policySet ??= _policyRepository.GetAllPolicies();
        _evaluationEngine = new EvaluationEngine(PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(_policySet));
    }

    public bool AddPolicy(PolicyDto policyDto)
    {
        _policySet.Policies ??= new();
        _policySet.Policies.Add(policyDto);
        _evaluationEngine = new EvaluationEngine(PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(_policySet));
        return _policyRepository.AddPolicy(policyDto);
    }

    public PolicySetDto GetPolicies()
    {
        return _policySet;
    }

    public bool DeletePolicy(string id)
    {
        var deleteResult = _policyRepository.DeletePolicy(id);

        if (!deleteResult) return deleteResult;

        _policySet = _policyRepository.GetAllPolicies();

        return deleteResult;
    }

    public XacmlContextResponse EvaluateRequest_V20(XacmlContextRequest xacmlContextRequest)
    {
        var xmlDocument = new XmlDocument();

        return _evaluationEngine.Evaluate(xacmlContextRequest, xmlDocument);
    }
}
