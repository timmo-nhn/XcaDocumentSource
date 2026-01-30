using XcaXds.Commons.Commons;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

public class JsonPolicyInputStrategy : IPolicyInputStrategy
{
    public bool CanHandle(string? contentType)
        => contentType == Constants.MimeTypes.Json;

    public Task<PolicyInputResult> BuildAsync(HttpContext context, string body)
    {
        var token = ExtractJwt(context.Request.Headers, out var ok);
        if (!ok || token == null)
            return Task.FromResult(Fail("Invalid or missing JWT"));

        var xacml = PolicyRequestMapperJsonWebTokenService
            .GetXacml20RequestFromJsonWebToken(token, null, context.Request.Path, context.Request.Method);

        return Task.FromResult(Success(xacml));
    }
}
