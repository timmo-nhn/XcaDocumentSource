using Microsoft.AspNetCore.Http;
using XcaXds.AtnaAuditLogging.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.AtnaAuditLogging.Services.AtnaAuditLogging.AtnaLogStrategies;

public interface IAtnaLogStrategy
{
    bool CanHandle(string path, string? contentType, string method);

    Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody);
}
