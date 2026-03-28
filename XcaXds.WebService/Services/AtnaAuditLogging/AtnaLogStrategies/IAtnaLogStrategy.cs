using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public interface IAtnaLogStrategy
{
    bool CanHandle(string contentType, string method);
    Task<AtnaLogBuilderResult> BuildAsync(HttpContext context);
}
