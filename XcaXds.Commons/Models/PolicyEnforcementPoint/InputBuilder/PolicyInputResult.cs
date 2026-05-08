using Abc.Xacml.Context;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;

namespace XcaXds.Commons.Models.PolicyEnforcementPoint.InputBuilder;

public class PolicyInputResult
{
    public PolicyInputResult() { }

    public PolicyInputResult(string message, bool? success = false)
    {
        ErrorMessage = success.HasValue && success.Value ? string.Empty : message;
    }

    public PolicyInputResult(XacmlContextRequest request)
    {
        IsSuccess = true;
        XacmlRequest = request;
    }

    public PolicyInputResult(XacmlContextRequest request, IPolicyInputStrategy policyInputStrategy)
    {
        IsSuccess = true;
        XacmlRequest = request;
        Strategy = policyInputStrategy;
    }

    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public XacmlContextRequest? XacmlRequest { get; init; }
    public IPolicyInputStrategy? Strategy { get; init; }


    public static PolicyInputResult Fail(string message)
    {
        return new PolicyInputResult(message, false);
    }

    public static PolicyInputResult Success(XacmlContextRequest xacmlRequest, IPolicyInputStrategy policyInputStrategy)
    {
        return new PolicyInputResult(xacmlRequest, policyInputStrategy);
    }
}
