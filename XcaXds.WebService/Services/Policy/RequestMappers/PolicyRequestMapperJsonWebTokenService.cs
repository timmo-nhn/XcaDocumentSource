using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared.Constants;
using XcaXds.WebService.Controllers;

namespace XcaXds.WebService.Services.Policy;

/// <summary>
/// Parse JSON Web Token (JWT) to an ABAC-request<para/>
/// Transforms JWT to a partial SAML-token and then to ABAC-Request Using <b>PolicyRequestMapperSamlService</b>
/// </summary>
// HAYO! Maybe refactor into Interface!!!
public class PolicyRequestMapperJsonWebTokenService
{
    private readonly ILogger<PolicyRequestMapperJsonWebTokenService> _logger;
    private readonly JwtToSamlTransformerService _jwtToSamlTransformerService;
    private readonly PolicyRequestMapperSamlService _policyRequestMapperSamlService;
    public PolicyRequestMapperJsonWebTokenService(ILogger<PolicyRequestMapperJsonWebTokenService> logger, JwtToSamlTransformerService jwtToSamlTransformerService, PolicyRequestMapperSamlService policyRequestMapperSamlService)
    {
        _logger = logger;
        _jwtToSamlTransformerService = jwtToSamlTransformerService;
        _policyRequestMapperSamlService = policyRequestMapperSamlService;   
    }

    public AbacRequest? GetAbacRequestFromJsonWebToken(JwtSecurityToken jwtToken, Resource? fhirBundle, string urlPath, string method)
    {
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