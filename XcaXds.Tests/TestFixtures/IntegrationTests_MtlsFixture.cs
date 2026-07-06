using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService;


namespace XcaXds.Tests;

#pragma warning disable CS8602, CS8604 // Dereference of a possibly null reference.
public class IntegrationTests_MtlsFixture : IAsyncLifetime
{
    public Uri BaseAddress { get; private set; } = default!;
    internal HttpClient Client { get; private set; } = default!;
    private WebApplication _app = default!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        Program.ConfigureKestrelAuthenticationAuthorization(builder);

        builder.WebHost.UseUrls("https://127.0.0.1:0");

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

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    private X509Certificate2 CreateX509Certificate()
    {
        GetTestDataFile("client.pfx", out var path);
        return X509CertificateLoader.LoadCertificate(File.ReadAllBytes(path));
    }

    private string GetTestDataFile(string v, out string? path)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var allFiles = Directory.GetFiles(testDataPath, "*", SearchOption.AllDirectories);
        path = allFiles.FirstOrDefault(f => f.Contains(v));
        return File.ReadAllText(path);
    }
}
#pragma warning restore CS8602, CS8604 // Dereference of a possibly null reference.