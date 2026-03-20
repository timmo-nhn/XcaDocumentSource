using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Middleware.AtnaAuditLogging;

internal class AtnaAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AtnaAuditLoggingMiddleware> _logger;

    public AtnaAuditLoggingMiddleware(RequestDelegate next, ILogger<AtnaAuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);
        if (IsMiddlewareEnabledForRequestEndpoint(httpContext))
        {
            _logger.LogInformation("ATNA audit logging is enabled for this endpoint. Logging request.");
        }
        else
        {
            _logger.LogInformation("ATNA audit logging is not enabled for this endpoint. Skipping logging.");
        }
    }

    private bool IsMiddlewareEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();
        return enforceAttr?.Enabled == true;
    }
}