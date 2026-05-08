using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.WebService.Controllers;

namespace XcaXds.WebService.Services;

/// <summary>
/// Parse Json Web Token (JWT) to an XACML-request<para/>
/// Transforms JWT to a partial SAML-token and then to XACML Using <b>PolicyRequestMapperSamlService</b> to ensure consistency with XACML-formats and policies
/// </summary>
public class PolicyRequestMapperJsonWebTokenService
{
    public XacmlContextRequest? GetXacml20RequestFromJsonWebToken(JwtSecurityToken jwtToken, Resource? fhirBundle, string urlPath, string path)
    {
		var (action, scopeToUse) = XacmlExtensions.MapXacmlActionAndScopeToUseFromUrlPath(urlPath, path);

		// The scopeToUse is used to pick a specific scope (based on the endpoint path) for the JWT to SAML transformation. 
		// This is because the XACML validation does not work if we add multiple attribute values to the Scope attribute, or add multiple Scope attributes
		// This may possibly be a bug or limitation in the Abc.Xacml library, but for now this is a workaround to ensure the correct scope is included in the SAML token for the XACML policies to work as intended.

		var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(jwtToken, scopeToUse);
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var samltokenAuthorizationAttributes = statements.Where(att =>
            att.Name.Contains("xacml") ||
            att.Name.Contains("xspa") ||
            att.Name.Contains("SecurityLevel") ||
            att.Name.Contains("Scope") ||
            att.Name.Contains("urn:ihe:iti") ||
            att.Name.Contains("acp") ||
            att.Name.Contains("provider-identifier"));        

        var appliesTo = SamlExtensions.GetIssuerEnumFromSamlToken(samlToken);

        var samlAttributes = PolicyRequestMapperSamlService.MapSamlAttributesToXacml20Properties(statements, action);
        var appliesToAttribute = PolicyRequestMapperSamlService.MapAppliesToToXacml20Properties(appliesTo);

        // Resource
        var xacmlResourceAttribute = samlAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id")).ToList();
            
        var xacmlResource = new XacmlContextResource(xacmlResourceAttribute);

		var actionAttribute = new XacmlContextAttribute(
            new Uri(Constants.Xacml.Attribute.ActionId),
            new Uri(Constants.Xacml.DataType.String),
            new XacmlContextAttributeValue() { Value = action });

        var xacmlAction = new XacmlContextAction(actionAttribute);

        // Subject
        var subjectAttributes = samlAttributes
            .Where(sa => sa.AttributeValues.All(av =>
                !string.IsNullOrWhiteSpace(av.Value)) &&
                (sa.AttributeId.OriginalString.Contains("subject") ||
                sa.AttributeId.OriginalString.Contains("acp"))).ToList();

        subjectAttributes.AddRange(appliesToAttribute);

        var xacmlSubject = new XacmlContextSubject(subjectAttributes);

        // Environment
        var xacmlEnvironment = new XacmlContextEnvironment();

        var contextRequest = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject, xacmlEnvironment);

        return contextRequest;
    }
}