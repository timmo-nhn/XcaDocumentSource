using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Claims;

namespace XcaXds.Commons.Models.Custom;

public static class CertificateValidator
{
    public static Task ValidateCertificate(CertificateValidatedContext context)
    {
        var cert = context.ClientCertificate;

        if (cert is null)
        {
            context.Fail("No client certificate.");
            return Task.CompletedTask;
        }

        // Example 1: check exact issuer
        const string expectedIssuer = "CN=NHN Internal CA - TEST, O=Norsk Helsenett SF, C=NO";
        if (!string.Equals(cert.Issuer, expectedIssuer, StringComparison.OrdinalIgnoreCase))
        {
            context.Fail("Invalid issuer.");
            return Task.CompletedTask;
        }

        // Example 2: optional thumbprint allow-list
        var allowedThumbprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Put known/allowed client cert thumbprints here
            // "ABC123..."
        };

        if (allowedThumbprints.Count > 0 && !allowedThumbprints.Contains(cert.Thumbprint))
        {
            context.Fail("Certificate not allowed.");
            return Task.CompletedTask;
        }

        // Example 3: optional subject / CN check
        // Your posted cert subject is CN=api.pjd.test.nhn.no
        if (!cert.Subject.Contains("CN=api.pjd.test.nhn.no", StringComparison.OrdinalIgnoreCase))
        {
            context.Fail("Unexpected subject.");
            return Task.CompletedTask;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, cert.Thumbprint ?? string.Empty),
            new Claim(ClaimTypes.Name, cert.Subject),
            new Claim("certificate_subject", cert.Subject),
            new Claim("certificate_thumbprint", cert.Thumbprint ?? string.Empty),
            new Claim("certificate_issuer", cert.Issuer)
        };

        context.Principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, context.Scheme.Name));

        context.Success();
        return Task.CompletedTask;

    }
}