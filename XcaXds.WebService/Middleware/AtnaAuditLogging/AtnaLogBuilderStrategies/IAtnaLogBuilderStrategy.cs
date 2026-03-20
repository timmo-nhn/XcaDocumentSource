using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilderStrategies;

public interface IAtnaLogBuilderStrategy
{
    bool CanHandle(string? urlPath, string httpMethod);
    Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry);
}
