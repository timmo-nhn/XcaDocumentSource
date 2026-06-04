using Microsoft.AspNetCore.Http;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.Commons.Models.PolicyEnforcementPoint.DenyStrategies;

public interface IPepDenyResponseStrategy
{
    string[] GetAcceptedContentTypes();
    bool CanHandle(string? contentType, PolicyInputResult input);
    Task WriteAsync(HttpContext context, PolicyInputResult input, ApplicationConfig appConfig, string? message);
}
