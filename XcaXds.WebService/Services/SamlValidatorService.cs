using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Cryptography.X509Certificates;

namespace XcaXds.WebService.Services;

public class Saml2Validator
{
    private readonly ILogger<Saml2Validator> _logger;
    private readonly Saml2SecurityTokenHandler _saml2Handler = new Saml2SecurityTokenHandler();
    private readonly ApplicationConfig _appConfig;
    private readonly SigningCertificateService _signingCertificateService;

    private string[]? SigningCertificates;
    private TokenValidationParameters ValidationParameters = default!;

    public Saml2Validator(ILogger<Saml2Validator> logger, ApplicationConfig applicationConfig, SigningCertificateService signingCertificateService)
    {
        _logger = logger;
        _appConfig = applicationConfig;
        _signingCertificateService = signingCertificateService;
    }

    public async Task<bool> InitValidatorIfNotInited()
    {
        if (ValidationParameters != null) return false;

        await _signingCertificateService.OverrideSigningCertificatesFromExternalApis();

        SigningCertificates = [_appConfig.HelseidCert, _appConfig.HelsenorgeCert];

        var idpCert = SigningCertificates.Select(cs => X509CertificateLoader.LoadCertificate(Convert.FromBase64String(cs)));
        var signingKeys = idpCert.Select(idpC => new X509SecurityKey(idpC));

        ValidationParameters = new TokenValidationParameters
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

        return true;
    }

    public bool ValidateSamlToken(string samlXml, out string? validationMessage)
    {
        validationMessage = string.Empty;
        var token = _saml2Handler.ReadSaml2Token(samlXml);
        try
        {
            var unescapedSamlToken = System.Text.RegularExpressions.Regex.Unescape(samlXml);
            var principal = _saml2Handler.ValidateToken(samlXml, ValidationParameters, out var validatedToken);
            //var principal = _saml2Handler.ValidateToken(unescapedSamlToken, _validationParameters, out var validatedToken); // Must use this for tokens from Kjernejournal portal? Tim: vi må diskutere dette nærmere
            var results = new List<bool>();

            foreach (var signingKey in ValidationParameters.IssuerSigningKeys)
            {
                var x509Key = (X509SecurityKey)signingKey;
                var chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                if (!chain.Build(x509Key.Certificate))
                {
                    validationMessage = string.Join(", ",
                        chain.ChainStatus.Select(s => $"{s.Status}: {s.StatusInformation}"));
                    validationMessage = $"Certificate chain invalid: {validationMessage}";
                    results.Add(false);
                }

                results.Add(true);
            }

            return results.Any(res => res == true);
        }
        catch (Exception ex)
        {
            validationMessage = ex.Message;
            return false;
        }
    }
}
