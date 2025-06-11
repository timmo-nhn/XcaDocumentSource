using Duende.IdentityModel.OidcClient.Browser;
using System.Diagnostics;
using System.Net;

namespace XcaXds.Source.Services.Custom;

public class SystemBrowser : IBrowser
{

    private readonly int _port;

    public SystemBrowser(int port)
    {
        _port = port;
    }
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{_port}/");
        listener.Start();

        Process.Start(new ProcessStartInfo
        {
            FileName = options.StartUrl,
            UseShellExecute = true
        });

        var context = await listener.GetContextAsync();
        var response = context.Response;
        var responseString = "<html><body>You can now return to the app.</body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();

        return new BrowserResult
        {
            ResultType = BrowserResultType.Success,
            Response = context.Request.Url.ToString()
        };
    }
}