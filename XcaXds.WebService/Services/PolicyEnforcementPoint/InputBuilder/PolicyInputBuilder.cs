using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint.InputBuilder;
using XcaXds.Shared.Extensions;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputBuilder;

public class PolicyInputBuilder
{
    private readonly IEnumerable<IPolicyInputStrategy> _strategies;
    private readonly ILogger<PolicyInputBuilder> _logger;

    public PolicyInputBuilder(IEnumerable<IPolicyInputStrategy> strategies, ILogger<PolicyInputBuilder> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public async Task<PolicyInputResult> BuildAsync(HttpContext ctx, ApplicationConfig appConfig)
    {
        if (!_strategies.Any())
        {
            throw new InvalidOperationException("Missing strategies");
        }

        var contentType = ctx.Request.ContentType?.Split(";").FirstOrDefault();
        var method = ctx.Request.Method;

        if (RequestMethodRequiresContentType(method) && string.IsNullOrWhiteSpace(contentType))
        {
            _logger.LogError("{traceIdentifier} - Missing content type", ctx.TraceIdentifier);
            return PolicyInputResult.Fail($"{ctx.TraceIdentifier} - Missing content type");
        }

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(contentType));
        if (strategy == null)
        {
            _logger.LogError("{traceIdentifier} - Unknown content type: {contentType}", ctx.TraceIdentifier, contentType);
            return PolicyInputResult.Fail($"{ctx.TraceIdentifier} - Unknown content type: {contentType}");
        }

        _logger.LogInformation("{traceIdentifier} - Content type: {contentType}", ctx.TraceIdentifier, contentType);
        return await strategy.BuildAsync(ctx, appConfig);
    }

    private static bool RequestMethodRequiresContentType(string method)
    {
        return method.IsAnyOf(
            HttpMethod.Post.Method,
            HttpMethod.Put.Method,
            HttpMethod.Patch.Method);
    }
}