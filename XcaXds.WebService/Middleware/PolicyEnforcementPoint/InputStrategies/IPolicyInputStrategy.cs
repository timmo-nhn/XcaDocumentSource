using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

public interface IPolicyInputStrategy
{
    string[] GetAcceptedContentTypes();
    bool CanHandle(string? contentType);
    Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry);
}
