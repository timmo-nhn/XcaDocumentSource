using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;

namespace XcaXds.WebService.Services;

/// <summary>
/// Parse Json Web Token (JWT) to an XACML-request<para/>
/// Transforms JWT to a partial SAML-token and then to XACML Using <b>PolicyRequestMapperSamlService</b> to ensure consistency with XACML-formats and policies
/// </summary>
public class PolicyRequestMapperJsonWebTokenService
{
    public XacmlContextRequest? GetXacml20RequestFromJsonWebToken(JwtSecurityToken jwtToken, Resource? fhirBundle, string urlPath, string path)
    {
        var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(jwtToken);
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var samltokenAuthorizationAttributes = statements.Where(att =>
            att.Name.Contains("xacml") ||
            att.Name.Contains("xspa") ||
            att.Name.Contains("SecurityLevel") ||
            att.Name.Contains("Scope") ||
            att.Name.Contains("urn:ihe:iti") ||
            att.Name.Contains("acp") ||
            att.Name.Contains("provider-identifier"));

        var action = MapXacmlActionFromUrlPath(urlPath, path);

        var samlAttributes = PolicyRequestMapperSamlService.MapSamlAttributesToXacml20Properties(statements, action);

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

        var xacmlSubject = new XacmlContextSubject(subjectAttributes);

        // Environment
        var xacmlEnvironment = new XacmlContextEnvironment();

        var contextRequest = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject, xacmlEnvironment);

        return contextRequest;
    }

    private static string MapXacmlActionFromUrlPath(string? urlPath, string method)
    {
        if (urlPath?.Equals("/R4/fhir/Bundle", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return Constants.Xacml.Actions.Create;

        if (urlPath?.StartsWith("/R4/fhir/Bundle", StringComparison.InvariantCultureIgnoreCase) == true && method == "PATCH")
            return Constants.Xacml.Actions.Update;

        if (urlPath?.Equals("/R4/fhir/mhd/document", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return Constants.Xacml.Actions.ReadDocuments;

        if (urlPath?.Equals("/R4/fhir/DocumentReference/_search", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return Constants.Xacml.Actions.ReadDocumentList;

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "GET")
            return Constants.Xacml.Actions.ReadDocumentList;

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "PATCH")
            return Constants.Xacml.Actions.Update;

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "DELETE")
            return Constants.Xacml.Actions.Delete;

        return Constants.Xacml.Actions.Create;
    }
}