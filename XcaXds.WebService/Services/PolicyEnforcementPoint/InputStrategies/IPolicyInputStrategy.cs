using XcaXds.WebService.Services.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public interface IPolicyInputStrategy
{
    string?[] GetAcceptedContentTypes();
    bool CanHandle(string? contentType);
    Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig);
}
