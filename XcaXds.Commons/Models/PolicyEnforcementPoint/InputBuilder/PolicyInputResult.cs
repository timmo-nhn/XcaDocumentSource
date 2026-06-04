using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.Custom;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.Commons.Models.PolicyEnforcementPoint.InputBuilder;

public class PolicyInputResult
{
    public PolicyInputResult() { }

    public PolicyInputResult(string message, bool? success = false)
    {
        ErrorMessage = success.HasValue && success.Value ? string.Empty : message;
    }

    public PolicyInputResult(AbacRequest request)
    {
        IsSuccess = true;
        AccessRequest = request;
    }

    public PolicyInputResult(AbacRequest request, IPolicyInputStrategy policyInputStrategy)
    {
        IsSuccess = true;
        AccessRequest = request;
        Strategy = policyInputStrategy;
    }

    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public AbacRequest? AccessRequest { get; init; }
    public IPolicyInputStrategy? Strategy { get; init; }
    
    public static PolicyInputResult Fail(string message)
    {
        return new PolicyInputResult(message, false);
    }

    public static PolicyInputResult Success(AbacRequest abacRequest, IPolicyInputStrategy policyInputStrategy)
    {
        return new PolicyInputResult(abacRequest, policyInputStrategy);
    }
}
