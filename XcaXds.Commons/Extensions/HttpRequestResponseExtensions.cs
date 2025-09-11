using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace XcaXds.Commons.Extensions;

public static class HttpRequestResponseExtensions
{
    public static async Task<string> GetHttpRequestBodyAsStringAsync(this HttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0; // Reset stream position for next reader
        return bodyContent;
    }

    public static async Task<string> ReadMultipartContentFromRequest(HttpContext httpContext)
    {
        var sb = new StringBuilder();

        if (!MediaTypeHeaderValue.TryParse(httpContext.Request.ContentType, out MediaTypeHeaderValue? mediaTypeHeaderValue)
        || !mediaTypeHeaderValue.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = HttpRequestResponseExtensions.GetBoundary(mediaTypeHeaderValue, 70);

            var multipartReader = new MultipartReader(boundary, httpContext.Request.Body);
            while (await multipartReader.ReadNextSectionAsync() is { } section)
            {
                using (var sr = new StreamReader(section.Body))
                {
                    sb.Append(await sr.ReadToEndAsync());
                }
            }
        }

        httpContext.Request.Body.Position = 0;
        return sb.ToString();
    }

    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
        if (StringSegment.IsNullOrEmpty(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }
        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
        }
        return boundary.ToString();
    }

}
