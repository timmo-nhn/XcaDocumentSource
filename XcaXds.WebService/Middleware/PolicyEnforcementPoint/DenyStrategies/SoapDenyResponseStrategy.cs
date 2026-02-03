using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyWriter;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;

public class SoapDenyResponseStrategy : IPepDenyResponseStrategy
{
    public bool CanHandle(string? contentType, PolicyInputResult input) =>
        GetAcceptedContentTypes().Contains(contentType);

    public async Task WriteAsync(HttpContext context, PolicyInputResult input, ApplicationConfig appConfig, string message)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var requestBody = await HttpRequestResponseExtensions.GetHttpRequestBodyAsStringAsync(context.Request);

        if (context.Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated)
        {
            requestBody = await MultipartExtensions.ReadMultipartContentFromRequest(context.Request);
        }


        var soapEnvelopeRequest = sxmls.DeserializeXmlString<SoapEnvelope>(requestBody);

        var soapEnvelopeResponse = new SoapEnvelope
        {
            Header = new()
            {
                Action = soapEnvelopeRequest.GetCorrespondingResponseAction(),
                MessageId = Guid.NewGuid().ToString(),
                RelatesTo = soapEnvelopeRequest.Header.MessageId,
            },
            Body = new()
        };

        var registryResponse = new RegistryResponseType();
        registryResponse.AddError(XdsErrorCodes.XDSRegistryError, message, appConfig.HomeCommunityId);

        SoapExtensions.PutRegistryResponseInTheCorrectPlaceAccordingToSoapAction(soapEnvelopeResponse, registryResponse);

        if (context.Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated)
        {
            context.Response.ContentType = Constants.MimeTypes.MultipartRelated;
            await WriteMultipartAsync(context, soapEnvelopeResponse);
        }
        else
        {
            context.Response.ContentType = Constants.MimeTypes.SoapXml;
            var xml = sxmls.SerializeSoapMessageToXmlString(soapEnvelopeResponse).Content ?? "";
            await context.Response.WriteAsync(xml);
        }
    }

    private static async Task WriteMultipartAsync(HttpContext context, SoapEnvelope envelope)
    {
        var soapMultipart = MultipartExtensions.ConvertRetrieveDocumentSetResponseToMultipartResponse(envelope, out var boundary);

        var contentId = soapMultipart.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var vals) == true
            ? vals.First().Trim('<', '>')
            : string.Empty;

        context.Response.ContentType =
            $"{Constants.MimeTypes.MultipartRelated}; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"";

        var bytes = await MultipartExtensions.SerializeMultipartAsync(soapMultipart);
        await context.Response.Body.WriteAsync(bytes);
    }

    public string[] GetAcceptedContentTypes()
    {
        return
        [
            Constants.MimeTypes.SoapXml,
            Constants.MimeTypes.Xml,
            Constants.MimeTypes.MultipartRelated
        ];
    }

}
