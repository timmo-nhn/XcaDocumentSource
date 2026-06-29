using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Cryptography.X509Certificates;

namespace XcaXds.WebService.Services;

public class SamlValidatorService
{
    private readonly ILogger<SamlValidatorService> _logger;
    private readonly Saml2SecurityTokenHandler _saml2Handler = new Saml2SecurityTokenHandler();
    private readonly ApplicationConfig _appConfig;
    private readonly SigningCertificateFetcherService _signingCertificateService;

    private string[]? SigningCertificates;
    private TokenValidationParameters ValidationParameters = default!;

    public SamlValidatorService(ILogger<SamlValidatorService> logger, ApplicationConfig applicationConfig, SigningCertificateFetcherService signingCertificateService)
    {
        _logger = logger;
        _appConfig = applicationConfig;
        _signingCertificateService = signingCertificateService;
    }

    public TokenValidationParameters GetSamlValidationParameters()
    {
        return ValidationParameters;
    }

    public async Task<SamlValidatorService> CreateSamlValidator()
    {
        if (ValidationParameters != null) return this;

        var certificates = await _signingCertificateService.GetSamlTokenSigningCertificatesFromExternalApis();

        SigningCertificates = certificates;

        var idpCert = SigningCertificates.Select(cs => X509CertificateLoader.LoadCertificate(Convert.FromBase64String(cs))).ToArray();
        var signingKeys = idpCert.Select(idpC => new X509SecurityKey(idpC)).ToArray();

        ValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.FromMinutes(5),
            ValidAudiences = _appConfig.SamlValidationValidAudiences,
            ValidIssuers = _appConfig.SamlValidationValidIssuers,

            IssuerSigningKeys = signingKeys,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
        };

        return this;
    }

    public bool ValidateSamlToken(string samlXml, out string? validationMessage)
    {
        validationMessage = string.Empty;
        var token = _saml2Handler.ReadSaml2Token(samlXml);

        try
        {
            var unescapedSamlToken = System.Text.RegularExpressions.Regex.Unescape(samlXml);
            var principal = _saml2Handler.ValidateToken(unescapedSamlToken, ValidationParameters, out var validatedToken);
            //var principal = _saml2Handler.ValidateToken(unescapedSamlToken, _validationParameters, out var validatedToken); // Must use this for tokens from Kjernejournal portal? Tim: vi må diskutere dette nærmere
            var results = new List<bool>();

            foreach (var signingKey in ValidationParameters.IssuerSigningKeys)
            {
                var x509Key = (X509SecurityKey)signingKey;
                var chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                if (!chain.Build(x509Key.Certificate))
                {
                    validationMessage = string.Join(", ", chain.ChainStatus.Select(s => $"{s.Status}: {s.StatusInformation}"));
                    validationMessage = $"Certificate chain invalid: {validationMessage}";
                    results.Add(false);
                    continue;
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
