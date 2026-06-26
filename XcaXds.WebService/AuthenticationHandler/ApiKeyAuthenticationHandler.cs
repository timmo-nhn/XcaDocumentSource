using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OpenTelemetry.Context;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using XcaXds.Commons.Models.Custom.ApiKey;
using XcaXds.Shared.Extensions;

namespace XcaXds.WebService.AuthenticationHandler;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiKeyHolder _apiKeyHolder;
    private readonly ApplicationConfig _appConfig;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ApiKeyHolder apiKeyHolder,
        ApplicationConfig appConfig)
        : base(options, logger, encoder)
    {
        _apiKeyHolder = apiKeyHolder;
        _appConfig = appConfig;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string headerName = "X-API-KEY";

        Claim[]? claims = null;

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        var apiKey = Request.Headers.TryGetValue(headerName, out var providedKey) ? providedKey.OfType<string>().ToArrayOrNull() : null;

        // Skip auth when no key is provided and config allows bypass or request is local;
        // if a key IS provided it must always be validated regardless of config/ip
        var shouldSkipAuth = apiKey == null && _appConfig.ApiKeyEnabled == false;

        if (shouldSkipAuth)
        {
            claims = [new Claim(ClaimTypes.Name, "BypassedApiKey")];
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        if (apiKey == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing API key"));
        }

        var expectedKey = _apiKeyHolder.ApiKey;

        if (providedKey != expectedKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
        }

        claims = [new Claim(ClaimTypes.Name, "ApiKeyUser")];

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
