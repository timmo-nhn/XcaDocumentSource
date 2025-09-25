using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace XcaXds.WebService.Services;

public class Saml2Validator
{
    private readonly Saml2SecurityTokenHandler _saml2Handler;
    private readonly TokenValidationParameters _validationParameters;

    public Saml2Validator(string cert)
    {
        _saml2Handler = new Saml2SecurityTokenHandler();

        var idpCert = new X509Certificate2(Convert.FromBase64String(cert));
        var signingKey = new X509SecurityKey(idpCert);

        _validationParameters = new TokenValidationParameters
        {

            ClockSkew = TimeSpan.FromMinutes(5),

            IssuerSigningKey = signingKey,

            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
        };
    }

    public bool ValidateSamlToken(string samlXml)
    {
        var token = _saml2Handler.ReadSaml2Token(samlXml);
        try
        {
            var principal = _saml2Handler.ValidateToken(samlXml, _validationParameters, out var validatedToken);

            var x509Key = (X509SecurityKey)_validationParameters.IssuerSigningKey;
            var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

            if (!chain.Build(x509Key.Certificate))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
