using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Middleware.AtnaAuditLogging.AtnaLogBuilderStrategies;

public interface IAtnaLogStrategy
{
    bool CanHandle(string contentType, string method);
    Task<AtnaLogBuilderResult> BuildAsync(HttpContext context);
}
