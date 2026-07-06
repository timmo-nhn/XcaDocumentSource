using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Buffers.Text;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;

namespace XcaXds.Commons.Extensions;

public static class MultipartExtensions
{
    public static async Task<byte[]> SerializeMultipartAsync(MultipartContent content)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        return ms.ToArray();
    }

    public static async Task<string> ReadMultipartContentFromStream(Stream body, string contentType)
    {
        var sb = new StringBuilder();

        if (!MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue? mediaTypeHeaderValue) ||
            !mediaTypeHeaderValue.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = GetMultipartBoundary(mediaTypeHeaderValue.Boundary.Value);

            var multipartReader = new MultipartReader(boundary, body);
            while (await multipartReader.ReadNextSectionAsync() is { } section)
            {
                using var sr = new StreamReader(section.Body);
                sb.Append(await sr.ReadToEndAsync());
            }
        }

        body.Position = 0;
        return sb.ToString();
    }

    public static async Task<SoapEnvelope> ReadFirstMultipartSectionSoapEnvelope(Stream body, string contentType)
    {
        var sxmls = new SoapXmlSerializer();
        return sxmls.DeserializeXmlString<SoapEnvelope>(await ReadFirstMultipartSectionFromStream(body, contentType));
    }

    public static async Task<string> ReadFirstMultipartSectionFromStream(Stream body, string contentType)
    {
        var sb = new StringBuilder();

        body.Position = 0;
        if (!MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue? mediaTypeHeaderValue) ||
            !mediaTypeHeaderValue.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = GetMultipartBoundary(mediaTypeHeaderValue.Boundary.Value);
            var multipartReaderTest = new SoapEnvelopeMultipartReader(boundary, body);

            var section = await multipartReaderTest.ReadNextSectionAsync();
            if (!(section?.Section?.Length > 0)) throw new InvalidOperationException("Response body is null");

            sb.Append(Encoding.Default.GetString(section.Section));
        }

        body.Position = 0;
        return sb.ToString();
    }

    private static string GetMultipartBoundary(string? boundary)
    {
        boundary = HeaderUtilities.RemoveQuotes(boundary).Value;
        if (string.IsNullOrEmpty(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        return boundary.ToString();
    }

    public static async Task<SoapEnvelope?> ReadMultipartSoapMessage(Stream? stream, string? contentTypeHeader)
    {
        if (MediaTypeHeaderValue.TryParse(contentTypeHeader, out var mediaTypeHeaderValue) &&
            !mediaTypeHeaderValue.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = mediaTypeHeaderValue.Boundary.Value?.Trim('"');

            if (boundary == null) return null;

            var multipartReader = new MultipartReader(boundary, stream);

            // using var multipartReader = new SoapEnvelopeMultipartReader(boundary, stream);
            //
            // var soapEnvelopeMultipart = new SoapEnvelopeMultipartResponse();
            // var sxmls = new SoapXmlSerializer();
            //
            // while(await multipartReader.ReadNextSectionAsync() is { } section)
            // {
            //     if (!(section.Section?.Length > 0)) continue;
            //
            //     var sectionString = Encoding.UTF8.GetString(section.Section);
            //
            //     if (GlobalExtensions.TryThis(() => sxmls.DeserializeXmlString<SoapEnvelope>(sectionString), out _))
            //     {
            //         soapEnvelopeMultipart.SoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(sectionString);
            //     }
            //     else
            //     {
            //         soapEnvelopeMultipart.MultiPartSections.Add(new() { ContentId = section.ContentId, Section = section.Section });
            //     }
            // }

            var structuredSoapEnvelopeMultiparts = await GetSoapEnvelopeMultipartSections(multipartReader);

            foreach (var documentResponse in structuredSoapEnvelopeMultiparts.SoapEnvelope?.Body.RetrieveDocumentSetResponse?.DocumentResponse ?? [])
            {
                var xopInclude = documentResponse.GetXmlDocumentAsXopInclude();
                documentResponse.SetInlineDocument(structuredSoapEnvelopeMultiparts.MultiPartSections
                    .FirstOrDefault(section => section.ContentId == xopInclude.href)?.Section ?? []);
            }

            return structuredSoapEnvelopeMultiparts.SoapEnvelope;
        }

        return null;
    }

    public static async Task<SoapEnvelope?> ReadMultipartSoapMessage(string contentTypeHeader, string messageString)
    {
        var bytes = Encoding.UTF8.GetBytes(messageString);
        using var stream = new MemoryStream(bytes);

        return await ReadMultipartSoapMessage(stream, contentTypeHeader);
    }


    private static async Task<SoapEnvelopeMultipartResponse> GetSoapEnvelopeMultipartSections(MultipartReader multipartReader)
    {
        var sxmls = new SoapXmlSerializer();

        var soapEnvelopeMultipart = new SoapEnvelopeMultipartResponse();
        while (await multipartReader.ReadNextSectionAsync() is { } section)
        {
            var contentId = $"cid:{section.Headers.GetValueOrDefault("Content-ID").ToString().TrimStart('<').TrimEnd('>')}";

            byte[] content;

            using (var sr = new StreamReader(section.Body))
            {
                content = Encoding.UTF8.GetBytes(sr.ReadToEnd());
            }

            var sectionString = Encoding.UTF8.GetString(content);

            if (GlobalExtensions.TryThis(() => sxmls.DeserializeXmlString<SoapEnvelope>(sectionString), out _))
            {
                soapEnvelopeMultipart.SoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(sectionString);
            }
            else
            {
                soapEnvelopeMultipart.MultiPartSections.Add(new() { ContentId = contentId, Section = content });
            }
        }

        return soapEnvelopeMultipart;
    }


    public static MultipartContent ConvertRetrieveDocumentSetRequestToMultipartRequest(SoapEnvelope soapEnvelope,
        out string boundary)
    {
        boundary = $"MIMEBoundary_{Guid.NewGuid().ToString().Replace("-", "")}";
        var multipart = new MultipartContent("related", boundary);

        var soapContent = GetSoapEnvelopeAsStringContent(soapEnvelope);

        if (soapContent != null)
        {
            multipart.Add(soapContent);
        }

        return multipart;
    }


    public static MultipartContent ConvertRetrieveDocumentSetResponseToMultipartResponse(SoapEnvelope soapEnvelope, out string boundary)
    {
        var documentResponses = soapEnvelope.Body.RetrieveDocumentSetResponse?.DocumentResponse;

        var documentContents = new List<HttpContent>();

        if (documentResponses != null)
        {
            foreach (var documentResponse in documentResponses)
            {
                if (string.IsNullOrWhiteSpace(documentResponse.Document?.InnerText)) continue;

                var documentBytes = Array.Empty<byte>();

                if (Base64.IsValid(documentResponse.Document.InnerText))
                {
                    var documentContent = Convert.FromBase64String(documentResponse.Document.InnerText);
                    documentBytes = new byte[documentContent.Length];
                    documentBytes = documentContent;
                }
                else
                {
                    var documentContent = Encoding.UTF8.GetBytes(documentResponse.Document.InnerText);
                    documentBytes = [.. documentContent];
                }

                documentResponse.MimeType = string.IsNullOrWhiteSpace(documentResponse.MimeType)
                    ? MimeTypeExtensions.TryGetMimeTypeFromDocumentBytes(documentBytes, out var mimeType)
                        ? mimeType
                        : documentResponse.MimeType
                    : documentResponse.MimeType;

                var documentByteArrayContent = new ByteArrayContent(documentBytes);

                var contentId = $"{Guid.NewGuid().ToString().Replace("-", "")}@nhn.no";

                documentByteArrayContent.Headers.ContentType = new(documentResponse.MimeType ?? string.Empty);

                documentByteArrayContent.Headers.Add("Content-ID", [$"<{contentId}>"]);
                documentByteArrayContent.Headers.Add("Content-Transfer-Encoding", "binary");

                documentContents.Add(documentByteArrayContent);

                // The corresponding <Include>-part in the DocumentResponse
                documentResponse.SetXopInclude($"cid:{contentId}");
            }
        }

        boundary = $"MIMEBoundary_{Guid.NewGuid().ToString().Replace("-", "")}";

        var multipart = new MultipartContent("related", boundary);

        var soapContent = GetSoapEnvelopeAsStringContent(soapEnvelope);
        if (soapContent != null)
        {
            multipart.Add(soapContent);
        }

        foreach (var docContent in documentContents)
            multipart.Add(docContent);

        multipart.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(Constants.MimeTypes.MultipartRelated,
                Encoding.UTF8.BodyName);

        return multipart;
    }


    private static StringContent? GetSoapEnvelopeAsStringContent(SoapEnvelope soapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline);

        var soapString = sxmls.SerializeSoapMessageToXmlString(soapEnvelope);
        if (string.IsNullOrWhiteSpace(soapString.Content)) return null;

        var stringContent = new StringContent(soapString.Content, Encoding.UTF8, Constants.MimeTypes.XopXml);
        stringContent.Headers.Add("Content-ID",
            [$"<{Guid.NewGuid().ToString().Replace("-", "")}@nhn.no>"]);
        stringContent.Headers.ContentType?.Parameters.Add(
            new System.Net.Http.Headers.NameValueHeaderValue("type", $"\"{Constants.MimeTypes.SoapXml}\""));
        stringContent.Headers.Add("Content-Transfer-Encoding", "binary");

        return stringContent;
    }
}