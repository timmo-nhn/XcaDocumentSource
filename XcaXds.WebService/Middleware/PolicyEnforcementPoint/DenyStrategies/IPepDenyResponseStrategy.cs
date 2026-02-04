using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyWriter;

public interface IPepDenyResponseStrategy
{
    string[] GetAcceptedContentTypes();
    bool CanHandle(string? contentType, PolicyInputResult input);
    Task WriteAsync(HttpContext context, PolicyInputResult input, ApplicationConfig appConfig, string message);
}
