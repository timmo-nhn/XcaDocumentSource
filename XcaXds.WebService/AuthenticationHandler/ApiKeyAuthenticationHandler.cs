using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.AuthenticationHandler;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiKeyHolder _apiKeyHolder;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ApiKeyHolder apiKeyHolder)
        : base(options, logger, encoder)
    {
        _apiKeyHolder = apiKeyHolder;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string headerName = "X-API-KEY";

        if (!Request.Headers.TryGetValue(headerName, out var providedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing API key"));
        }

        var expectedKey = _apiKeyHolder.ApiKey;

        if (providedKey != expectedKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKeyUser")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
