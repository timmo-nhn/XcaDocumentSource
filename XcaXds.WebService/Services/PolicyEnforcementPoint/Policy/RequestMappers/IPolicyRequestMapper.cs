namespace XcaXds.WebService.Services.PolicyEnforcementPoint.Policy.RequestMappers;

public interface IPolicyRequestMapper<TInput>
{
    AbacRequest? MapToAbacRequest(TInput? input);
}