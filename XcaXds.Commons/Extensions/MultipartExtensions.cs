using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Buffers.Text;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Extensions;

public static class MultipartExtensions
{
    public static async Task<byte[]> SerializeMultipartAsync(MultipartContent content)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        return ms.ToArray();
    }


    public static async Task<string> ReadMultipartContentFromRequest(HttpRequest httpRequest)
    {
        var sb = new StringBuilder();

        if (!MediaTypeHeaderValue.TryParse(httpRequest.ContentType, out MediaTypeHeaderValue? mediaTypeHeaderValue)
        || !mediaTypeHeaderValue.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = GetMultipartBoundary(mediaTypeHeaderValue.Boundary.Value, 512);

            var multipartReader = new MultipartReader(boundary, httpRequest.Body);
            while (await multipartReader.ReadNextSectionAsync() is { } section)
            {
                using (var sr = new StreamReader(section.Body))
                {
                    sb.Append(await sr.ReadToEndAsync());
                }
            }
        }

        httpRequest.Body.Position = 0;
        return sb.ToString();
    }


    public static string GetMultipartBoundary(string boundary, int lengthLimit)
    {
        boundary = HeaderUtilities.RemoveQuotes(boundary).Value;
        if (string.IsNullOrEmpty(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }
        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
        }
        return boundary.ToString();
    }


    public static async Task<SoapEnvelope?> ReadMultipartSoapMessage(string contentType, string messageString)
    {
        SoapEnvelope? soapMultipartMessage = new();

        if (!MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue? mediaTypeHeaderValue)
        || !mediaTypeHeaderValue.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(messageString);
                writer.Flush();
                stream.Position = 0;

                var multipartReader = new MultipartReader(mediaTypeHeaderValue.Boundary.Value?.Trim('"'), stream);

                var structuredSoapEnvelopeMultiparts = await GetSoapEnvelopeMultipartSections(multipartReader);

                foreach (var documentResponse in structuredSoapEnvelopeMultiparts.SoapEnvelope?.Body?.RetrieveDocumentSetResponse?.DocumentResponse ?? [])
                {
                    var xopInclude = documentResponse.GetXmlDocumentAsXopInclude();
                    documentResponse.SetInlineDocument(structuredSoapEnvelopeMultiparts.MultiPartSections.FirstOrDefault(section => section.ContentId == xopInclude.href)?.Section ?? []);
                }

                soapMultipartMessage = structuredSoapEnvelopeMultiparts.SoapEnvelope;
            }
        }

        return soapMultipartMessage;
    }


    private static async Task<SoapEnvelopeMultipartResponse> GetSoapEnvelopeMultipartSections(MultipartReader multipartReader)
    {
        var envelope = new SoapEnvelope();
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

            if (GlobalExtensions.TryThis(() => sxmls.DeserializeXmlString<SoapEnvelope>(sectionString)))
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


    public static MultipartContent ConvertRetrieveDocumentSetRequestToMultipartRequest(SoapEnvelope soapEnvelope, out string boundary)
    {
        boundary = $"MIMEBoundary_{Guid.NewGuid().ToString().Replace("-", "")}";
        var multipart = new MultipartContent("related", boundary);

        var soapContent = GetSoapEnvelopeAsStringContent(soapEnvelope);
        multipart.Add(soapContent);

        return multipart;
    }


    public static MultipartContent ConvertRetrieveDocumentSetResponseToMultipartResponse(SoapEnvelope soapEnvelope, out string boundary)
    {
        var documentResponses = soapEnvelope.Body.RetrieveDocumentSetResponse?.DocumentResponse;
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline);

        var documentContents = new List<HttpContent>();

        if (documentResponses != null)
        {
            foreach (var documentResponse in documentResponses)
            {
                if (string.IsNullOrWhiteSpace(documentResponse.Document?.InnerText)) continue;

                var documentBytes = new byte[0];

                if (Base64.IsValid(documentResponse.Document.InnerText) && documentResponse.MimeType == Constants.MimeTypes.Hl7v3Xml)
                {
                    var documentContent = Convert.FromBase64String(documentResponse.Document.InnerText);
                    documentBytes = new byte[documentContent.Length];
                    documentBytes = documentContent;
                }
                else
                {
                    var documentContent = Encoding.UTF8.GetBytes(documentResponse.Document.InnerText);
                    documentBytes = new byte[documentContent.Length];
                    documentBytes = documentContent;
                }


                var documentByteArrayContent = new ByteArrayContent(documentBytes);

                var contentId = $"{Guid.NewGuid().ToString().Replace("-", "")}@xcadocumentsource.com";

                documentByteArrayContent.Headers.ContentType = new(documentResponse.MimeType);

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
        multipart.Add(soapContent);

        foreach (var docContent in documentContents)
            multipart.Add(docContent);

        multipart.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Constants.MimeTypes.MultipartRelated, Encoding.UTF8.BodyName);

        return multipart;
    }


    private static StringContent GetSoapEnvelopeAsStringContent(SoapEnvelope soapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline);

        var soapString = sxmls.SerializeSoapMessageToXmlString(soapEnvelope);
        var stringContent = new StringContent(soapString.Content, Encoding.UTF8, Constants.MimeTypes.XopXml);
        stringContent.Headers.Add("Content-ID", [$"<{Guid.NewGuid().ToString().Replace("-", "")}@xcadocumentsource.com>"]);
        stringContent.Headers.ContentType?.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("type", $"\"{Constants.MimeTypes.SoapXml}\""));
        stringContent.Headers.Add("Content-Transfer-Encoding", "binary");

        return stringContent;
    }

}
