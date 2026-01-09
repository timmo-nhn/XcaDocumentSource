using Microsoft.AspNetCore.Http;

public static class HttpRequestResponseExtensions
{
    public static async Task<string> GetHttpRequestBodyAsStringAsync(HttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0; // Reset stream position for next reader
        return bodyContent;
    }
}
