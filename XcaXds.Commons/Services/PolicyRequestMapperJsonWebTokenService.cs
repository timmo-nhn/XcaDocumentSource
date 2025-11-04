using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.WebUtilities;
using System.IdentityModel.Tokens.Jwt;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;

namespace XcaXds.Commons.Services;


public class PolicyRequestMapperJsonWebTokenService
{
    //FIXME maybe some day, do something about JWT aswell?!
    public static XacmlContextRequest? GetXacml20RequestFromJsonWebToken(JwtSecurityToken jwtToken, Resource fhirBundle)
    {
        var jwtAttributes = MapJsonWebTokenClaimsToXacml20Properties(jwtToken);

        XacmlContextRequest contextRequest = null;
        return contextRequest;
    }

    private static List<XacmlContextAttribute> MapJsonWebTokenClaimsToXacml20Properties(JwtSecurityToken jwtToken)
    {
        var subjectAttributes = new List<XacmlContextAttribute>();

        var payload = jwtToken.Payload;

        var claims = new Dictionary<string, string>();
        foreach (var claim in payload)
        {
            claims.Add(claim.Key, claim.Value.ToString());
        }

        var samlClaims = SamlTrustFrameworkClaimsMapper.GetClaimValues(claims);



        foreach (var item in payload)
        {
            if (item.Value is string stringItem)
            {
                subjectAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(item.Key),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = stringItem }));
            }
        }

        return subjectAttributes;
    }
}