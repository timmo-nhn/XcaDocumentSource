using System.IdentityModel.Tokens.Jwt;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.Helpers;

public static class JwtExtractor
{
    public static JwtSecurityToken? ExtractJwt(IHeaderDictionary headers, out bool success)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwtToken = headers["Authorization"].FirstOrDefault();
        var canRead = handler.CanReadToken(jwtToken);

        if (canRead == false)
        {
            success = false;
            return null;
        }

        success = true;
        return handler.ReadJwtToken(jwtToken);
    }
}