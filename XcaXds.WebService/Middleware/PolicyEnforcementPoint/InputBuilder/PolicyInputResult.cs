using Abc.Xacml.Context;
using XcaXds.Commons.Commons;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

public class PolicyInputResult
{
    public PolicyInputResult() { }

    public PolicyInputResult(string message, bool? success = false)
    {
        ErrorMessage = success.HasValue && success.Value ? string.Empty : message;
    }

    public PolicyInputResult(XacmlContextRequest request, Issuer appliesTo)
    {
        IsSuccess = true;
        XacmlRequest = request;
        AppliesTo = appliesTo;
    }

    public PolicyInputResult(XacmlContextRequest request, Issuer appliesTo, IPolicyInputStrategy policyInputStrategy)
    {
        IsSuccess = true;
        XacmlRequest = request;
        AppliesTo = appliesTo;
        Strategy = policyInputStrategy;
    }

    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public XacmlContextRequest? XacmlRequest { get; init; }
    public Issuer AppliesTo { get; init; } = Issuer.Unknown;
    public IPolicyInputStrategy Strategy { get; init; }


    public static PolicyInputResult Fail(string message)
    {
        return new PolicyInputResult(message, false);
    }

    public static PolicyInputResult Success(XacmlContextRequest xacmlRequest, Issuer appliesTo, IPolicyInputStrategy policyInputStrategy)
    {
        return new PolicyInputResult(xacmlRequest, appliesTo, policyInputStrategy);
    }
}
