using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace XcaXds.Commons.Extensions;

public static class HttpRequestResponseExtensions
{
    public static async Task<string> GetHttpRequestBodyAsStringAsync(this HttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0; // Reset stream position for next reader
        if (bodyContent.StartsWith("--MIMEBoundary"))
        {
            bodyContent = await ReadMultipartContentFromRequest(bodyContent);
        }
        return bodyContent;
    }

    public static async Task<string> ReadMultipartContentFromRequest(string bodyContent)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(bodyContent.Replace("\r\r\n", "\r\n")));
        var boundary = "MIMEBoundary_cab1d70f258a850f002aa2ab5645aaa9622f017d951cfe8d";

        var reader = new MultipartReader(boundary, stream);

        MultipartSection? section;

        var wholeContent = string.Empty;

        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            var contentType = section.ContentType;
            using var sr = new StreamReader(section.Body, Encoding.UTF8);
            wholeContent += await sr.ReadToEndAsync();

        }
        return wholeContent;
    }
}
