using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.WebService.Controllers;

namespace XcaXds.WebService.Services;

/// <summary>
/// Parse JSON Web Token (JWT) to an ABAC-request<para/>
/// Transforms JWT to a partial SAML-token and then to ABAC-Request Using <b>PolicyRequestMapperSamlService</b>
/// </summary>
public class PolicyRequestMapperJsonWebTokenService
{
    public AbacRequest? GetAbacRequestFromJsonWebToken(JwtSecurityToken jwtToken, Resource? fhirBundle, string urlPath, string method)
    {
        var abacRequest = new AbacRequest();
		var action = AccessControlExtensions.MapXacmlActionFromUrlPath(urlPath, method);

		var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(jwtToken);
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var appliesTo = SamlExtensions.GetIssuerEnumFromSamlToken(samlToken);

        var samlAttributes = PolicyRequestMapperSamlService.MapSamlAttributesToAbacProperties(statements);
        abacRequest.Attributes.AddRange(samlAttributes);
        
        var appliesToAttribute = PolicyRequestMapperSamlService.MapAppliesToToAbacProperties(appliesTo);
        abacRequest.Attributes.AddRange(appliesToAttribute);
        
        abacRequest.Attributes.Add(Constants.Xacml.Attribute.ActionId, [action]);
        
        return abacRequest;
    }
}