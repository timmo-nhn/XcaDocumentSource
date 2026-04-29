using XcaXds.Commons.Attributes;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Middleware;

internal class AtnaAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AtnaAuditLoggingMiddleware> _logger;

    public AtnaAuditLoggingMiddleware(RequestDelegate next, ILogger<AtnaAuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, AtnaLogBuilder atnaLogBuilder)
    {
        var originalResponseBodyStream = httpContext.Response.Body;

        // The request body is disposed after the middleware has run,
        // so we need to keep it here
        await using var requestBody = new MemoryStream();
        httpContext.Request.Body.CopyTo(requestBody);
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        await _next(httpContext);

        responseBody.Seek(0, SeekOrigin.Begin);

        if (IsMiddlewareEnabledForRequestEndpoint(httpContext))
        {
            _logger.LogInformation("ATNA Audit Logging is enabled for this endpoint. Logging request.");
            if (httpContext.Request.Body.CanSeek)
            {
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            await atnaLogBuilder.BuildAsync(httpContext, requestBody);

            responseBody.Seek(0, SeekOrigin.Begin);
        }

        await responseBody.CopyToAsync(originalResponseBodyStream);

        httpContext.Response.Body = originalResponseBodyStream;
    }

    private bool IsMiddlewareEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<ExportsAtnaAuditLogAttribute>();
        return enforceAttr?.Enabled == true;
    }
}