using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Shared;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.Policy.RequestMappers;

/// <summary>
/// Parse JSON Web Token (JWT) to an ABAC-request<para/>
/// Transforms JWT to a partial SAML-token and then to ABAC-Request Using <b>PolicyRequestMapperSamlService</b>
/// </summary>
// HAYO! Maybe refactor into Interface!!!
public class JsonWebTokenPolicyRequestMapper : IPolicyRequestMapper<JwtRequestMapperInput>
{
    private readonly ILogger<JsonWebTokenPolicyRequestMapper> _logger;
    private readonly JwtToSamlTransformerService _jwtToSamlTransformerService;
    private readonly SamlPolicyRequestMapper _policyRequestMapperSamlService;

    public JsonWebTokenPolicyRequestMapper(ILogger<JsonWebTokenPolicyRequestMapper> logger, JwtToSamlTransformerService jwtToSamlTransformerService, SamlPolicyRequestMapper policyRequestMapperSamlService)
    {
        _logger = logger;
        _jwtToSamlTransformerService = jwtToSamlTransformerService;
        _policyRequestMapperSamlService = policyRequestMapperSamlService;
    }

    public AbacRequest? MapToAbacRequest(JwtRequestMapperInput? input)
    {
        if (input == null) return null;

        var jwtToken = input.JwtToken;
        var urlPath = input.UrlPath;
        var method = input.Method;

        var abacRequest = new AbacRequest();
        var action = AccessControlExtensions.MapXacmlActionFromUrlPath(urlPath, method);

        var samlToken = _jwtToSamlTransformerService.MapJsonWebTokenToSamlToken(jwtToken);
        var statements = samlToken.GetAllStatements();

        var appliesTo = SamlExtensions.GetIssuerEnumFromSamlToken(samlToken);

        var samlAttributes = _policyRequestMapperSamlService.MapSamlAttributesToAbacProperties(statements);
        abacRequest.Attributes.AddRange(samlAttributes);

        var appliesToAttribute = _policyRequestMapperSamlService.MapAppliesToToAbacProperties(appliesTo);
        abacRequest.Attributes.AddRange(appliesToAttribute);

        abacRequest.Attributes.Add(Constants.Xacml.Attribute.ActionId, [action]);

        return abacRequest;
    }
}