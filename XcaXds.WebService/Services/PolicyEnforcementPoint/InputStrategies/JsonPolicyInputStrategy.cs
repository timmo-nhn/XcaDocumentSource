using XcaXds.Commons.Commons;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint.InputBuilder;
using XcaXds.Shared;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy.RequestMappers;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public class JsonPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly JsonWebTokenPolicyRequestMapper _policyRequestMapperJsonWebTokenService;
    public JsonPolicyInputStrategy(JsonWebTokenPolicyRequestMapper policyRequestMapperJsonWebTokenService)
    {
        _policyRequestMapperJsonWebTokenService = policyRequestMapperJsonWebTokenService;
    }

    public string[] GetAcceptedContentTypes()
    {
        return
        [
            Constants.MimeTypes.Json
        ];
    }

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);

    public async Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig)
    {
        var token = JwtExtractor.ExtractJwt(context.Request.Headers, out var ok);
        if (!ok || token == null)
            return PolicyInputResult.Fail("Invalid or missing JWT");

        var abacRequest = _policyRequestMapperJsonWebTokenService.MapToAbacRequest(new JwtRequestMapperInput(token, null, context.Request.Path, context.Request.Method)) ?? 
                          throw new InvalidOperationException("Failed to create ABAC request from JWT");
        
        if (abacRequest == null)
        {
            return PolicyInputResult.Fail($"Error generating ABAC request from SOAP Envelope");
        }

        return PolicyInputResult.Success(abacRequest, this);
    }
}
