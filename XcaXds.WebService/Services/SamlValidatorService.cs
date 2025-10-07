using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Cryptography.X509Certificates;

namespace XcaXds.WebService.Services;

public class Saml2Validator
{
    private readonly Saml2SecurityTokenHandler _saml2Handler;
    private readonly TokenValidationParameters _validationParameters;

    public Saml2Validator(string[] signingCertificates)
    {
        _saml2Handler = new Saml2SecurityTokenHandler();

        if (signingCertificates == null)
        {
            throw new Exception("Signing certificate missing! SAML-token cannot be validated!");
        }

        var idpCert = signingCertificates.Select(cs => new X509Certificate2(Convert.FromBase64String(cs)));
        var signingKeys = idpCert.Select(idpC => new X509SecurityKey(idpC));

        _validationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromMinutes(5),
            ValidAudiences = ["https://ptr1xds-reg.prod.drift.nhn.no/", "https://xds-web.test.nhn.no/", "nhn:dokumentdeling-saml"],
            ValidIssuers = ["https://helseid-xdssaml.prod.drift.nhn.no", "https://helseid-xdssaml.test.nhn.no", "sikkerhet.helsenorge.no"],

            IssuerSigningKeys = signingKeys,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
        };
    }

    public bool ValidateSamlToken(string samlXml, out string? validationMessage)
    {
        validationMessage = string.Empty;
        var token = _saml2Handler.ReadSaml2Token(samlXml);
        try
        {
            var principal = _saml2Handler.ValidateToken(samlXml, _validationParameters, out var validatedToken);

            var x509Key = (X509SecurityKey)_validationParameters.IssuerSigningKey;
            var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

            if (!chain.Build(x509Key.Certificate))
            {
                validationMessage = string.Join(", ",
                    chain.ChainStatus.Select(s => $"{s.Status}: {s.StatusInformation}"));
                validationMessage = $"Certificate chain invalid: {validationMessage}";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            validationMessage = ex.Message;
            return false;
        }
    }
}
