using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Middleware;

internal class AtnaAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AtnaAuditLoggingMiddleware> _logger;
    private readonly AtnaLogBuilderService _atnaLogBuilder;

    public AtnaAuditLoggingMiddleware(RequestDelegate next, ILogger<AtnaAuditLoggingMiddleware> logger, AtnaLogBuilderService atnaLogBuilder)
    {
        _next = next;
        _logger = logger;
        _atnaLogBuilder = atnaLogBuilder;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);
        if (IsMiddlewareEnabledForRequestEndpoint(httpContext))
        {
            _logger.LogInformation("ATNA audit logging is enabled for this endpoint. Logging request.");
            await _atnaLogBuilder.BuildAsync(httpContext);
        }
        else
        {
            _logger.LogInformation("ATNA audit logging is not enabled for this endpoint. Skipping logging.");
        }
    }

    private bool IsMiddlewareEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<ExportsAtnaAuditLogAttribute>();
        return enforceAttr?.Enabled == true;
    }
}