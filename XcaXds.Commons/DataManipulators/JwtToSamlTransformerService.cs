using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Extensions.No;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared.Extensions;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;

namespace XcaXds.Commons.DataManipulators
{
    public class JwtToSamlTransformerService
    {
        private readonly ILogger<JwtToSamlTransformerService> _logger;
        private readonly TerminologyService _terminologyService;
        private readonly INinParser _ninParser;

        public JwtToSamlTransformerService(
            ILogger<JwtToSamlTransformerService> logger,
            TerminologyService terminologyService,
            INinParser ninParser)
        {
            _logger = logger;
            _terminologyService = terminologyService;
            _ninParser = ninParser;
        }

        public Saml2SecurityToken MapJsonWebTokenToSamlToken(JwtSecurityToken jwtToken)
        {
            var payload = jwtToken.Payload;

            var claims = new Dictionary<string, string>();
            var scopes = new List<string>(); // Treat scopes as a special case since they can contain multiple values (from JWT token) or be a single value (from SAML token)

            foreach (var claim in payload)
            {
                if (claim.Value == null)
                {
                    continue;
                }

                string[] claimsEnumerated = claim.Value switch
                {
                    IEnumerable<string> stringEnumerable => [.. stringEnumerable],
                    System.Text.Json.JsonElement jsonElement => jsonElement.ValueKind switch
                    {
                        System.Text.Json.JsonValueKind.Array => jsonElement.EnumerateArray().Select(je => je.ToString()).ToArray(),
                        _ => [jsonElement.ToString()]
                    },
                    _ => [claim.Value.ToString() ?? ""]
                };


                foreach (var singleClaim in claimsEnumerated)
                {
                    if (!string.IsNullOrWhiteSpace(singleClaim))
                    {
                        if (claim.Key == "scope")
                        {
                            scopes.Add(singleClaim);
                        }
                        else
                        {
                            claims[claim.Key] = singleClaim;
                        }
                    }
                }
            }

            var samlTrustFrameworkClaims = SamlTrustFrameworkClaimsMapper.GetClaimValues(claims, scopes);

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
            if (!string.IsNullOrEmpty(samlTrustFrameworkClaims.NameId))
            {
                token.Assertion.Subject.NameId = new Saml2NameIdentifier(samlTrustFrameworkClaims.NameId);
            }

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

        private List<Saml2Statement> MapJwtClaimsToSamlTokenStatements(SamlClaimValues samlClaims)
        {
            var samlAttributes = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Authentication.SamlAttributes);

            var statements = new List<Saml2Statement>();
            if (!string.IsNullOrWhiteSpace(samlClaims.NameId))
            {
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.OrgnrParent))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "helseid://claims/client/claims/orgnr_parent",
                    samlClaims.OrgnrParent)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.ClientName))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "helseid://claims/client/client_name",
                    samlClaims.ClientName)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.Pid))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "helseid://claims/identity/pid",
                    samlClaims.Pid)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.HprNumber))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "helseid://claims/hpr/hpr_number",
                    samlClaims.HprNumber)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.Name))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "name",
                    samlClaims.Name)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.GivenName))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "given_name",
                    samlClaims.GivenName)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.MiddleName))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "middle_name",
                    samlClaims.MiddleName)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.FamilyName))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    "family_name",
                    samlClaims.FamilyName)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.SubjectId))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.SubjectId,
                    samlAttributes.GetByName("SubjectId"),
                    samlClaims.SubjectId)));
            }
            else
            {
                var composedName = string.Join(' ', new[] { samlClaims.GivenName, samlClaims.MiddleName, samlClaims.FamilyName }
                    .Where(p => !string.IsNullOrWhiteSpace(p)));

                if (!string.IsNullOrWhiteSpace(composedName))
                {
                    statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                        //Constants.Saml.Attribute.SubjectId,
                        samlAttributes.GetByName("SubjectId"),
                        composedName)));
                }
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.HprNumber) && string.IsNullOrWhiteSpace(samlClaims.ProviderIdentifier))
            {
                samlClaims.ProviderIdentifier = samlClaims.HprNumber;
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.RoleCode))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.Role,
                    samlAttributes.GetByName("Role"),
                    MapAttributesToHl7XmlAttribute(samlClaims.RoleCode, samlClaims.RoleCodeSystem, samlClaims.RoleCodeSystemName, samlClaims.RoleCodeName, "Role", "CE"))));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.HomeCommunityId))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.EhelseHomeCommunityId,
                    samlAttributes.GetByName("EhelseHomeCommunityId"),
                    samlClaims.HomeCommunityId)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.Npi))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.Npi,
                    samlAttributes.GetByName("Npi"),
                    samlClaims.Npi)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.PurposeOfUseCode))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.PurposeOfUse,
                    samlAttributes.GetByName("PurposeOfUse"),
                    MapAttributesToHl7XmlAttribute(samlClaims.PurposeOfUseCode, samlClaims.PurposeOfUseCodeSystem, samlClaims.PurposeOfUseAuthorityName, samlClaims.PurposeOfUseDescription, "PurposeOfUse", "CE"))));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.Organization))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.PurposeOfUse,
                    samlAttributes.GetByName("Organization"),
                    samlClaims.Organization)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.OrganizationId))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.PurposeOfUse,
                    samlAttributes.GetByName("OrganizationId"),
                    MapAttributesToHl7XmlAttribute(samlClaims.OrganizationId, samlClaims.OrganizationCodeSystem, samlClaims.OrganizationAuthority, null, "id", "II"))));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.ChildOrganizationName))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.TrustChildOrgName,
                    samlAttributes.GetByName("TrustChildOrgName"),
                    samlClaims.ChildOrganizationName)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.ChildOrganization))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.ChildOrganization,
                    samlAttributes.GetByName("ChildOrganization"),
                    MapAttributesToHl7XmlAttribute(samlClaims.ChildOrganization, samlClaims.ChildOrganizationCodeSystem, samlClaims.ChildOrganizationAuthority, null, "id", "II"))));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.PatientChildOrganization))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.TrustResourceChildOrg,
                    samlAttributes.GetByName("TrustResourceChildOrg"),
                    MapAttributesToHl7XmlAttribute(samlClaims.PatientChildOrganization, samlClaims.PatientChildOrganizationCodeSystem, samlClaims.PatientChildOrganizationAuthority, null, "id", "II"))));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.ResourceId))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.ResourceId20,
                    samlAttributes.GetByName("ResourceId20"),
                    MapResourceClaimToSamlAttributeValue(samlClaims))));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.SecurityLevel))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.EhelseSecurityLevel,
                    samlAttributes.GetByName("SecurityLevel"),
                    samlClaims.SecurityLevel)));
            }

            var scopeAttribute = samlAttributes.GetByName("Scope");
            var clientIdAttribute = samlAttributes.GetByName("ClientId");
            var authenticationMethodAttribute = samlAttributes.GetByName("AuthenticationMethod");
            var healthcareServiceAttribute = samlAttributes.GetByName("HealthcareService");
            var organizationAttribute = samlAttributes.GetByName("Organization");
            var bppcAttribute = samlAttributes.GetByName("BppcDocId");
            var xuaAcpAttribute = samlAttributes.GetByName("XuaAcp");
            
            var bppcNullValue = _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Authentication.Bppc, "NullValue")?.FirstOrDefault();
            var xuaAcpNullValue = _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Authentication.Acp, "NullValue")?.FirstOrDefault();
            
            foreach (var scope in samlClaims.Scope ?? [])
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                        scopeAttribute, scope)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.ClientId))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    clientIdAttribute,
                    samlClaims.ClientId)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.AuthenticationMethod))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    authenticationMethodAttribute,
                    samlClaims.AuthenticationMethod)));
            }

            if (!string.IsNullOrWhiteSpace(samlClaims.Organization))
            {
                statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    organizationAttribute,
                    samlClaims.AuthenticationMethod)));
            }

            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(bppcAttribute, bppcNullValue)));
            statements.Add(new Saml2AttributeStatement(new Saml2Attribute(xuaAcpAttribute, xuaAcpNullValue)));

            return statements;
        }

        private string? MapResourceClaimToSamlAttributeValue(SamlClaimValues samlClaims)
        {
            var patientIdCx = _ninParser.ParseNinToCxWithAssigningAuthority(samlClaims.ResourceId);
            return patientIdCx?.Serialize();
        }

        private static string MapAttributesToHl7XmlAttribute(string code, string? codeSystem, string? codeSystemName, string? displayName, string xmlName, string xsiType)
        {
            var displayAttr = string.IsNullOrWhiteSpace(displayName)
                ? "displayable=\"false\""
                : $"displayName=\"{displayName}\"";

            return $"<{xmlName} xmlns=\"urn:hl7-org:v3\" xsi:type=\"{xsiType}\" code=\"{code}\" codeSystem=\"{codeSystem}\" codeSystemName=\"{codeSystemName}\" {displayAttr}/>";
        }
    }
}