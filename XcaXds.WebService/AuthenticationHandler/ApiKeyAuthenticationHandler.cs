using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using XcaXds.Commons.Models.Custom.ApiKey;

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

        Claim[]? claims = null;

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        var isLocal = Context.Connection.RemoteIpAddress is { } ip &&
              (IPAddress.IsLoopback(ip) || ip.Equals(Context.Connection.LocalIpAddress));

        if (isLocal)
        {
            claims = [new Claim(ClaimTypes.Name, "LocalUser")];
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        if (!Request.Headers.TryGetValue(headerName, out var providedKey))
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
