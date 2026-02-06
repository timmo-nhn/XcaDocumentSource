using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

public class PolicyInputBuilder
{
    private readonly IEnumerable<IPolicyInputStrategy> _strategies;

    public PolicyInputBuilder(IEnumerable<IPolicyInputStrategy> strategies)
        => _strategies = strategies;

    public async Task<PolicyInputResult>BuildAsync(HttpContext ctx, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry)
    {
        if (!_strategies.Any())
        {
            throw new InvalidOperationException("Missing strategies");
        }

        var contentType = ctx.Request.ContentType?.Split(";").FirstOrDefault();

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(contentType));
        if (strategy == null)
            return PolicyInputResult.Fail($"Unknown content type: {contentType}");

        return await strategy.BuildAsync(ctx, appConfig, documentRegistry);
    }
}