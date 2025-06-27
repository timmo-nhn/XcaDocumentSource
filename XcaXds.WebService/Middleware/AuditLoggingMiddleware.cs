namespace XcaXds.WebService.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddlware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<PolicyEnforcementPointMiddlware> logger, ApplicationConfig xdsConfig, IWebHostEnvironment env)
    {
        _logger = logger;
        _next = next;
        _xdsConfig = xdsConfig;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);
    }
}