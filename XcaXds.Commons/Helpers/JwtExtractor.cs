using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace XcaXds.Commons.Helpers;

public static class JwtExtractor
{
    public static JwtSecurityToken? ExtractJwt(string? jwtToken, out bool success)
    {
        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(jwtToken);

        if (canRead == false)
        {
            success = false;
            return null;
        }

        success = true;
        return handler.ReadJwtToken(jwtToken);
    }

    public static JwtSecurityToken? ExtractJwt(IHeaderDictionary headers, out bool success)
    {
        var jwtToken = headers["Authorization"].FirstOrDefault();

        return ExtractJwt(jwtToken, out success);
    }
}