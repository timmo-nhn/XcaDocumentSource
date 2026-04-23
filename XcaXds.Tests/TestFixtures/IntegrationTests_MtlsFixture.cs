using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace XcaXds.Tests;

#pragma warning disable CS8602, CS8604 // Dereference of a possibly null reference.
public class IntegrationTests_MtlsFixture : IAsyncLifetime
{
    public Uri BaseAddress { get; private set; } = default!;
    internal HttpClient Client { get; private set; } = default!;
    private WebApplication _app = default!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseKestrel();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;

                httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
                {
                    return true; 
                };
            });
        });

        builder.WebHost.UseUrls("https://127.0.0.1:0");

        builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(options =>
            {
                options.AllowedCertificateTypes = CertificateTypes.All;

                options.Events = new CertificateAuthenticationEvents
                {
                    OnCertificateValidated = context =>
                    {
                        context.Success();
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(WebService.Program).Assembly);

        _app = builder.Build();

        _app.UseRouting();

        _app.UseAuthentication();
        _app.UseAuthorization();
        
        _app.MapControllers();

        await _app.StartAsync();

        BaseAddress = new Uri(_app.Urls.First());

        var cert = CreateX509Certificate();

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert);
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        Client = new HttpClient(handler)
        {
            BaseAddress = BaseAddress
        };

        Console.WriteLine(_app.Urls.First());
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    private X509Certificate2 CreateX509Certificate()
    {
        using var rsa = RSA.Create(2048);

        var req = new CertificateRequest(
            "CN=trusted-client",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        // IMPORTANT: mark as client auth
        req.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection
                {
                new("1.3.6.1.5.5.7.3.2") // Client Authentication
                },
                false));

        var cert = req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));

        // IMPORTANT: persist key properly for TLS stack
        return new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
    }

    private string GetTestDataFile(string v)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var allFiles = Directory.GetFiles(testDataPath, "*", SearchOption.AllDirectories);

        return File.ReadAllText(allFiles.First(f => f.Contains(v)));
    }
}
#pragma warning restore CS8602, CS8604 // Dereference of a possibly null reference.