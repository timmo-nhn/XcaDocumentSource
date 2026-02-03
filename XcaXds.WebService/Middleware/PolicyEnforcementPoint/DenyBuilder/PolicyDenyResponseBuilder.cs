using XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyWriter;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyBuilder;

public class PolicyDenyResponseBuilder
{
    private readonly IEnumerable<IPepDenyResponseStrategy> _strategies;

    public PolicyDenyResponseBuilder(IEnumerable<IPepDenyResponseStrategy> strategies)
        => _strategies = strategies;

    public Task WriteAsync(HttpContext ctx, PolicyInputResult input, ApplicationConfig appConfig, string message)
    {
        var contentType = ctx.Request.ContentType?.Split(";").FirstOrDefault();
        var strategy = _strategies.First(s => s.CanHandle(contentType, input));
        return strategy.WriteAsync(ctx, input, appConfig, message);
    }
}
