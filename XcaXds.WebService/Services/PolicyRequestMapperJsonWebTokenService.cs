using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
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
		var (action, scopeToUse) = MapXacmlActionAndScopeToUseFromUrlPath(urlPath, path);

		// The scopeToUse is used to pick a specific scope (based on the endpoint path) for the JWT to SAML transformation. 
		// This is because the XCAML validation does not work if we add multiple attribute values to the Scope attribute, or add multiple Scope attributes
		// This may possibly be a bug or limitation in the Abc.Xcaml library, but for now this is a workaround to ensure the correct scope is included in the SAML token for the XACML policies to work as intended.

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

    private static (string action, string? scopeToUse) MapXacmlActionAndScopeToUseFromUrlPath(string? urlPath, string method)
    {
        if (urlPath?.Equals("/R4/fhir/Bundle", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.Create, FhirMobileAccessToHealthDocumentsController.Scopes.ScopeCreateDocuments);

        if (urlPath?.StartsWith("/R4/fhir/Bundle", StringComparison.InvariantCultureIgnoreCase) == true && method == "PATCH")
            return (Constants.Xacml.Actions.Update, FhirMobileAccessToHealthDocumentsController.Scopes.ScopeCreateDocuments);

        if (urlPath?.Equals("/R4/fhir/mhd/document", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.ReadDocuments, null);

        if (urlPath?.Equals("/R4/fhir/DocumentReference/_search", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.ReadDocumentList, null);

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "GET")
            return (Constants.Xacml.Actions.ReadDocumentList, null);

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "PATCH")
            return (Constants.Xacml.Actions.Update, FhirMobileAccessToHealthDocumentsController.Scopes.ScopeCreateDocuments);

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "DELETE")
            return (Constants.Xacml.Actions.Delete, FhirMobileAccessToHealthDocumentsController.Scopes.ScopeDeleteDocument);

        return (Constants.Xacml.Actions.Create, null);
    }
}