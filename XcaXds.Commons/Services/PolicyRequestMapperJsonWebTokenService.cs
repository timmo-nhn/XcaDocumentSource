using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Commons.Services;

/// <summary>
/// Parse Json Web Token (JWT) to an XACML-request<para/>
/// Transforms JWT to a partial SAML-token and then to XACML Using <b>PolicyRequestMapperSamlService</b> to ensure consistency with XACML-formats and policies
/// </summary>
public class PolicyRequestMapperJsonWebTokenService
{
    public static XacmlContextRequest? GetXacml20RequestFromJsonWebToken(JwtSecurityToken jwtToken, Resource fhirBundle, string urlPath, string path)
    {
        var samlToken = MapJsonWebTokenToSamlToken(jwtToken);
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
            new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = action });

        var xacmlAction = new XacmlContextAction(actionAttribute);

        // Subject
        var subjectAttributes = samlAttributes.Where(sa => sa.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)) && (sa.AttributeId.OriginalString.Contains("subject") || sa.AttributeId.OriginalString.Contains("acp"))).ToList();

        var xacmlSubject = new XacmlContextSubject(subjectAttributes);

        // Environment
        var xacmlEnvironment = new XacmlContextEnvironment();

        var contextRequest = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject, xacmlEnvironment);

        return contextRequest;
    }

    private static string MapXacmlActionFromUrlPath(string? urlPath, string method)
    {
        if (urlPath == "R4/fhir/Bundle" && method == "POST")
            return Constants.Xacml.Actions.Create;

        if (urlPath == "R4/fhir/mhd/document" && method == "POST")
            return Constants.Xacml.Actions.ReadDocuments;

        if (urlPath == "R4/fhir/DocumentReference/_search" && method == "POST")
            return Constants.Xacml.Actions.ReadDocumentList;

        if (urlPath == "R4/fhir/DocumentReference" && method == "GET")
            return Constants.Xacml.Actions.ReadDocumentList;

        if (urlPath == "R4/fhir/DocumentReference" && method == "DELETE")
            return Constants.Xacml.Actions.Delete;

        return Constants.Xacml.Actions.Create;
    }

    private static Saml2SecurityToken MapJsonWebTokenToSamlToken(JwtSecurityToken jwtToken)
    {
        var subjectAttributes = new List<XacmlContextAttribute>();

        var payload = jwtToken.Payload;

        var claims = new Dictionary<string, string>();

        foreach (var claim in payload)
        {
            claims.Add(claim.Key, claim.Value.ToString());
        }

        var samlTrustFrameworkClaims = SamlTrustFrameworkClaimsMapper.GetClaimValues(claims);

        var issuer = claims.GetValueOrDefault("iss");

        var audience = claims.GetValueOrDefault("aud");

        var descriptor = new SecurityTokenDescriptor
        {
            Audience = audience,
            IssuedAt = DateTime.Now,
            NotBefore = DateTime.Now,
            Expires = DateTime.Now.AddMinutes(60),
            Issuer = issuer,
            Subject = new System.Security.Claims.ClaimsIdentity(),
        };

        var handler = new Saml2SecurityTokenHandler();
        var token = (Saml2SecurityToken)handler.CreateToken(descriptor);
        token.Assertion.Subject.NameId = new Saml2NameIdentifier(samlTrustFrameworkClaims.NameId);
        var samlStatements = MapJwtClaimsToSamlTokenStatements(samlTrustFrameworkClaims);

        foreach (var statement in samlStatements)
        {
            token.Assertion.Statements.Add(statement);
        }

        var authTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();

        token.Assertion.Statements.Add(GetAuthenticationStatement(authTime));

        return token;
    }

    private static Saml2AuthenticationStatement GetAuthenticationStatement(string authTime)
    {
        var x509ClassReference = new Uri("urn:oasis:names:tc:SAML:2.0:ac:classes:X509");
        var authnContext = new Saml2AuthenticationContext(x509ClassReference);

        var authenticationTime = GetAuthenticationTime(authTime);
        return new Saml2AuthenticationStatement(authnContext, authenticationTime)
        {
            SessionNotOnOrAfter = DateTime.Now.AddMinutes(60).TruncateMilliseconds()
        };
    }

    private static DateTime GetAuthenticationTime(string authTime)
    {
        return DateTimeOffset
            .FromUnixTimeSeconds(long.Parse(authTime))
            .LocalDateTime
            .TruncateMilliseconds();
    }

    private static List<Saml2Statement> MapJwtClaimsToSamlTokenStatements(SamlClaimValues samlClaims)
    {
        var statements = new List<Saml2Statement>();

        if (!string.IsNullOrEmpty(samlClaims.SubjectId))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.SubjectId,
                samlClaims.SubjectId)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.RoleCode))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.Role,
                MapAttributesToHl7XmlAttribute(samlClaims.RoleCode, samlClaims.RoleCodeSystem, samlClaims.RoleCodeSystemName, samlClaims.RoleCodeName, "Role", "CE"))));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.HomeCommunityId))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.EhelseHomeCommunityId,
                samlClaims.HomeCommunityId)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.Npi))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.Npi,
                samlClaims.Npi)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.PurposeOfUseCode))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.PurposeOfUse,
                MapAttributesToHl7XmlAttribute(samlClaims.PurposeOfUseCode, samlClaims.PurposeOfUseCodeSystem, samlClaims.PurposeOfUseAuthorityName, samlClaims.PurposeOfUseDescription, "PurposeOfUse", "CE"))));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.Organization))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.PurposeOfUse,
                samlClaims.Organization)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.OrganizationId))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.PurposeOfUse,
                MapAttributesToHl7XmlAttribute(samlClaims.OrganizationId, samlClaims.OrganizationCodeSystem, samlClaims.OrganizationAuthority, null, "id", "II"))));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.ChildOrganizationName))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.TrustChildOrgName,
                samlClaims.ChildOrganizationName)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.ChildOrganization))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.ChildOrganization,
                MapAttributesToHl7XmlAttribute(samlClaims.ChildOrganization, samlClaims.ChildOrganizationCodeSystem, samlClaims.ChildOrganizationAuthority, null, "id", "II"))));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.PatientChildOrganization))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.TrustResourceChildOrg,
                MapAttributesToHl7XmlAttribute(samlClaims.PatientChildOrganization, samlClaims.PatientChildOrganizationCodeSystem, samlClaims.PatientChildOrganizationAuthority, null, "id", "II"))));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.ResourceId))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.ResourceId20,
                MapResourceClaimToSamlAttributeValue(samlClaims))));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.SecurityLevel))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.EhelseSecurityLevel,
                samlClaims.SecurityLevel)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.Scope))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.EhelseScope, samlClaims.Scope)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.ClientId))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.EhelseClientId,
                samlClaims.ClientId)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.AuthenticationMethod))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.EhelseAuthenticationMethod,
                samlClaims.AuthenticationMethod)));
        }

        if (!string.IsNullOrWhiteSpace(samlClaims.Organization))
        {
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                Constants.Saml.Attribute.Organization,
                samlClaims.AuthenticationMethod)));
        }

        statements.Add(new Saml2AttributeStatement(new Saml2Attribute(Constants.Saml.Attribute.BppcDocId, Constants.Oid.Saml.Bppc.NullValue)));
        statements.Add(new Saml2AttributeStatement(new Saml2Attribute(Constants.Saml.Attribute.XuaAcp, Constants.Oid.Saml.Acp.NullValue)));

        return statements;
    }

    private static string? MapResourceClaimToSamlAttributeValue(SamlClaimValues samlClaims)
    {
        var patientIdCx = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(samlClaims.ResourceId);
        return patientIdCx?.Serialize();
    }

    private static string MapAttributesToHl7XmlAttribute(string code, string codeSystem, string codeSystemName, string? displayName, string xmlName, string xsiType)
    {
        var displayAttr = string.IsNullOrWhiteSpace(displayName)
            ? "displayable=\"false\""
            : $"displayName=\"{displayName}\"";

        return $"<{xmlName} xmlns=\"urn:hl7-org:v3\" xsi:type=\"{xsiType}\" code=\"{code}\" codeSystem=\"{codeSystem}\" codeSystemName=\"{codeSystemName}\" {displayAttr}/>";
    }
}