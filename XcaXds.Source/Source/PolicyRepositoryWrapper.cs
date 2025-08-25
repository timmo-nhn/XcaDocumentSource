using Abc.Xacml.Context;
using Abc.Xacml.Runtime;
using Microsoft.Extensions.Logging;
using System.Xml;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Services;

namespace XcaXds.Source.Source;

public class PolicyRepositoryWrapper
{
    internal PolicySetDto _policySet;

    internal EvaluationEngine _evaluationEngine;

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

    public PolicyDto? GetPolicy(string id)
    {
        return _policySet.Policies?.FirstOrDefault(pol => pol.Id == id);
    }

    public bool DeletePolicy(string id)
    {
        var deleteResult = _policyRepository.DeletePolicy(id);

        if (!deleteResult) return deleteResult;

        _policySet = _policyRepository.GetAllPolicies();
        _evaluationEngine = new EvaluationEngine(PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(_policySet));
        return deleteResult;
    }

    public XacmlContextResponse EvaluateRequest_V20(XacmlContextRequest xacmlContextRequest)
    {
        var xmlDocument = new XmlDocument();

        return _evaluationEngine.Evaluate(xacmlContextRequest, xmlDocument);
    }
}
