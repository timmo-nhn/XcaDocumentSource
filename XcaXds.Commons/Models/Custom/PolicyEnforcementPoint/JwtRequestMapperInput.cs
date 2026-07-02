using Hl7.Fhir.Model;
using System.IdentityModel.Tokens.Jwt;

namespace XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;

public class JwtRequestMapperInput
{
    public JwtSecurityToken JwtToken { get; }
    public Resource? FhirBundle { get; }
    public string UrlPath { get; }
    public string Method { get; }

    public JwtRequestMapperInput(JwtSecurityToken jwtToken, Resource? fhirBundle, string urlPath, string method)
    {
        JwtToken = jwtToken;
        FhirBundle = fhirBundle;
        UrlPath = urlPath;
        Method = method;
    }
}