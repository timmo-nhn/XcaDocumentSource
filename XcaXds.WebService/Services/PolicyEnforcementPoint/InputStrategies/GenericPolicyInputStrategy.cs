using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public class GenericPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly PolicyRequestMapperJsonWebTokenService _policyRequestMapperJsonWebTokenService;
    public GenericPolicyInputStrategy(PolicyRequestMapperJsonWebTokenService policyRequestMapperJsonWebTokenService)
    {
        _policyRequestMapperJsonWebTokenService = policyRequestMapperJsonWebTokenService;
    }

    public string?[] GetAcceptedContentTypes()
    {
        return [null];
    }

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);

    public async Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig)
    {
        var token = JwtExtractor.ExtractJwt(context.Request.Headers, out var ok);
        if (!ok || token == null)
            return PolicyInputResult.Fail("Invalid or missing JWT");

        var xacml = _policyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(token, null, context.Request.Path, context.Request.Method) ??
            throw new InvalidOperationException("Failed to create XACML request from JWT.");

        return PolicyInputResult.Success(xacml, this);
    }
}
