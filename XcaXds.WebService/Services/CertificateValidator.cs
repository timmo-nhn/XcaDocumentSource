using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Claims;

namespace XcaXds.Commons.Models.Custom;

public static class CertificateValidator
{
    public static async Task ValidateCertificate(CertificateValidatedContext context)
    {
        var cert = context.ClientCertificate;

        if (cert is null)
        {
            context.Fail("No client certificate.");
            return;
        }

        var expectedIssuer = "CN=NHN Internal CA - TEST, O=Norsk Helsenett SF, C=NO";
        if (!string.Equals(cert.Issuer, expectedIssuer, StringComparison.OrdinalIgnoreCase))
        {
            context.Fail("Invalid issuer.");
            return;
        }

        var now = DateTime.UtcNow;

        if (now < cert.NotBefore || now > cert.NotAfter)
        {
            context.Fail("Certificate expired or not yet valid");
            return;
        }

        var allowedThumbprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "67140A9E628C81D22F12E5F687C6B695E9E7095E",
        };

        if (!allowedThumbprints.Contains(cert.Thumbprint))
        {
            context.Fail("Unknown client certificate");
            return;
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
        return;
    }
}