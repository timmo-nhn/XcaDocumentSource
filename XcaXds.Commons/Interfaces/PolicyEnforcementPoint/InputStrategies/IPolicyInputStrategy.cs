using Microsoft.AspNetCore.Http;
using XcaXds.Commons.Models.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;

public interface IPolicyInputStrategy
{
    string?[] GetAcceptedContentTypes();
    bool CanHandle(string? contentType);
    Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig);
}
