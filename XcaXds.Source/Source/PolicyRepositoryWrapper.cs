using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;
using Microsoft.Extensions.Logging;
using System.Xml;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Source.Source;

public class PolicyRepositoryWrapper
{
    internal volatile XacmlPolicySet _policies = null;

    private readonly IPolicyRepository _policyRepository;

    private readonly ILogger<PolicyRepositoryWrapper> _logger;

    private EvaluationEngine _evaluationEngine;
    private EvaluationEngine30 _evaluationEngine30;

    public PolicyRepositoryWrapper(IPolicyRepository policyRepository, ILogger<PolicyRepositoryWrapper> logger)
    {
        _logger = logger;
        _policyRepository = policyRepository;
        _policies ??= _policyRepository.GetAllPolicies();
        _evaluationEngine = new EvaluationEngine(_policies);
        _evaluationEngine30 = new EvaluationEngine30(_policies);
    }

    public PolicyRepositoryWrapper(FileBasedPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
        _policies ??= _policyRepository.GetAllPolicies();
        _evaluationEngine = new EvaluationEngine(_policies);
        _evaluationEngine30 = new EvaluationEngine30(_policies);
    }

    public bool AddPolicy(XacmlPolicy xacmlPolicy)
    {
        return _policyRepository.AddPolicy(xacmlPolicy);
    }

    public XacmlContextResponse? EvaluateReqeust_V20(XacmlContextRequest? xacmlContextRequest)
    {
        var xmlDocument = new XmlDocument();

        return _evaluationEngine.Evaluate(xacmlContextRequest, xmlDocument);
    }

    public XacmlContextResponse? EvaluateRequest_V30(XacmlContextRequest? xacmlContextRequest)
    {
        var xmlDocument = new XmlDocument();

        return _evaluationEngine30.Evaluate(xacmlContextRequest, xmlDocument);
    }
}
