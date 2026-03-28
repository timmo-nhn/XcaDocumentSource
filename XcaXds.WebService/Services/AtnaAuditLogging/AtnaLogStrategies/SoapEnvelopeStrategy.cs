using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class SoapEnvelopeStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    public SoapEnvelopeStrategy(AtnaLogGeneratorService atnaLogGeneratorService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
    }

    public bool CanHandle(string contentType, string method)
    {
        return contentType.IsAnyOf(Constants.MimeTypes.SoapXml, Constants.MimeTypes.MultipartRelated) && method == "POST";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var request = context.Request;
        var response = context.Response;

        var requestBody = await HttpRequestResponseExtensions.GetStreamAsStringAsync(request.Body);

        if (request.ContentType != null && request.ContentType.Contains(Constants.MimeTypes.MultipartRelated))
        {
            requestBody = await MultipartExtensions.ReadMultipartContentFromStream(request.Body, request.ContentType);
        }

        var responseBody = await HttpRequestResponseExtensions.GetStreamAsStringAsync(response.Body);

        if (response.ContentType != null && response.ContentType.Contains(Constants.MimeTypes.MultipartRelated))
        {
            responseBody = await MultipartExtensions.ReadMultipartContentFromStream(response.Body, response.ContentType);
        }

        var requestSoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(requestBody);
        var responseSoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(responseBody);

        if (requestSoapEnvelope == null || responseSoapEnvelope == null)
        {
            var requestOrResponseOrBoth = (requestSoapEnvelope, responseSoapEnvelope) switch
            {
                (null, null) => "Request and Response",
                (null, _) => "Request",
                (_, null) => "Response",
                _ => ""
            };

            return AtnaLogBuilderResult.Fail($"{context.TraceIdentifier} - Failed to deserialize SOAP {requestOrResponseOrBoth} body");
        }

        _atnaLogGeneratorService.CreateAuditLogForSoapRequestResponse(requestSoapEnvelope, responseSoapEnvelope);
        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}