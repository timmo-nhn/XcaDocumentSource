using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.DataManipulators;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.Helpers;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

public class FhirJsonPolicyInputStrategy : IPolicyInputStrategy
{
    public string[] GetAcceptedContentTypes()
    {
        return
        [
            Constants.MimeTypes.FhirJson
        ];
    }

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);

    public async Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry)
    {
        var token = JwtExtractor.ExtractJwt(context.Request.Headers, out var ok);
        if (!ok || token == null)
            return PolicyInputResult.Fail("Invalid or missing JWT");

        var xacml = PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(token, null, context.Request.Path, context.Request.Method);

        return PolicyInputResult.Success(xacml, Issuer.HelseId, this);
    }
}
