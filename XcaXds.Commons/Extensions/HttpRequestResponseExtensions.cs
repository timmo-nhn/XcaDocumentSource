using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;

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
            var boundary = GetBoundary(mediaTypeHeaderValue, 512);

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


    public static MultipartContent ConvertToMultipartResponse(SoapEnvelope soapEnvelope, out string boundary)
    {
        var documentResponses = soapEnvelope.Body.RetrieveDocumentSetResponse?.DocumentResponse;
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var documentContents = new List<HttpContent>();

        if (documentResponses != null)
        {
            foreach (var documentResponse in documentResponses)
            {
                // The multipart section content
                var documentString = documentResponse.Document.InnerText;
                var stringContent = new StringContent(documentString, Encoding.UTF8, documentResponse.MimeType);
                
                var contentId = $"{documentResponse.GetHashCode()}@xcadocumentsource.com";

                stringContent.Headers.Add("Content-ID", [$"<{contentId}>"]);

                documentContents.Add(stringContent);

                // The corresponding <Include>-part in the DocumentResponse
                documentResponse.SetXopInclude($"cid:{contentId}");

                var gobb = sxmls.SerializeSoapMessageToXmlString(documentResponse);
            }
        }

        var soapString = sxmls.SerializeSoapMessageToXmlString(soapEnvelope);
        var soapContent = new StringContent(soapString.Content, Encoding.UTF8, Constants.MimeTypes.XopXml);
        soapContent.Headers.Add("Content-ID", [$"<{soapEnvelope.GetHashCode()}@xcadocumentsource.com>"]);
        soapContent.Headers.ContentType?.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("type", $"\"{Constants.MimeTypes.SoapXml}\""));
        soapContent.Headers.Add("Content-Transfer-Encoding", "binary");

        boundary = $"MIMEBoundary_{Guid.NewGuid().ToString().Replace("-", "")}";

        var multipart = new MultipartContent("related", boundary);
       
        multipart.Add(soapContent);

        foreach (var docContent in documentContents)
            multipart.Add(docContent);

        multipart.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Constants.MimeTypes.MultipartRelated, Encoding.UTF8.BodyName);

        return multipart;
    }
}
