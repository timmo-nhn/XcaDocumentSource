using XcaXds.Commons.Commons;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint.InputBuilder;
using XcaXds.Shared;
using XcaXds.WebService.Services.Policy;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public class FhirJsonPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly PolicyRequestMapperJsonWebTokenService _policyRequestMapperJsonWebTokenService;
    public FhirJsonPolicyInputStrategy(PolicyRequestMapperJsonWebTokenService policyRequestMapperJsonWebTokenService)
    {
        _policyRequestMapperJsonWebTokenService = policyRequestMapperJsonWebTokenService;
    }

    public string[] GetAcceptedContentTypes()
    {
        return
        [
            Constants.MimeTypes.FhirJson
        ];
    }

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);

    public async Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig)
    {
        var token = JwtExtractor.ExtractJwt(context.Request.Headers, out var ok);
        if (!ok || token == null)
            return PolicyInputResult.Fail("Invalid or missing JWT");

        var abacRequest = _policyRequestMapperJsonWebTokenService.GetAbacRequestFromJsonWebToken(token, null, context.Request.Path, context.Request.Method) ?? 
            throw new InvalidOperationException("Failed to create ABAC request from JWT");

        if (abacRequest == null)
        {
            return PolicyInputResult.Fail($"Error generating ABAC request from SOAP Envelope");
        }

        return PolicyInputResult.Success(abacRequest, this);
    }
}
