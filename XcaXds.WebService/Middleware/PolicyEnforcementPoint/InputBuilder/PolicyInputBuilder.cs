using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

public class PolicyInputBuilder
{
    private readonly IEnumerable<IPolicyInputStrategy> _strategies;
    private readonly ILogger<PolicyInputBuilder> _logger;

    public PolicyInputBuilder(IEnumerable<IPolicyInputStrategy> strategies, ILogger<PolicyInputBuilder> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public async Task<PolicyInputResult> BuildAsync(HttpContext ctx, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry)
    {
        if (!_strategies.Any())
        {
            throw new InvalidOperationException("Missing strategies");
        }

        var contentType = ctx.Request.ContentType?.Split(";").FirstOrDefault();

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(contentType));
        if (strategy == null)
        {
            _logger.LogError($"Unknown content type: {contentType}");
            return PolicyInputResult.Fail($"Unknown content type: {contentType}");
        }

        _logger.LogError($"Content type: {contentType}");
        return await strategy.BuildAsync(ctx, appConfig, documentRegistry);
    }
}