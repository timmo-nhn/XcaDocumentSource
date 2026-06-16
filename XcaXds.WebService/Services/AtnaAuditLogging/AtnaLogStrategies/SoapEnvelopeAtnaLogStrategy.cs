using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Shared.Constants;
using XcaXds.Shared.Extensions;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class SoapEnvelopeAtnaLogStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    public SoapEnvelopeAtnaLogStrategy(AtnaLogGeneratorService atnaLogGeneratorService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return contentType.IsAnyOf(Constants.MimeTypes.SoapXml, Constants.MimeTypes.MultipartRelated) && method == "POST";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var request = context.Request;
        var response = context.Response;

        var requestBodyString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(requestBody);

        if (request.ContentType != null && request.ContentType.Contains(Constants.MimeTypes.MultipartRelated))
        {
            requestBodyString = await MultipartExtensions.ReadMultipartContentFromStream(requestBody, request.ContentType);
        }

        var responseBodyString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(responseBody);

        if (response.ContentType != null && response.ContentType.Contains(Constants.MimeTypes.MultipartRelated))
        {
            responseBodyString = await MultipartExtensions.ReadFirstMultipartSectionFromStream(responseBody, response.ContentType);
        }

        var requestSoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(requestBodyString);
        var responseSoapEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(responseBodyString);

        var pdpDecision = context.Items.TryGetValue("pdpDecision", out var decision) ? decision as AccessControlResponse : null;
        var businessLogicResult = (context.Items.TryGetValue("businessLogicResult", out var cast) ? cast : null) as Dictionary<string, int>;
        var deletedEntries = (context.Items.TryGetValue("deletedRegistryObjects", out var deleted) ? deleted : null) as IEnumerable<RegistryObjectDto>;


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

        var additionalParameters = new AdditionalParameters(context.Request.Method, context.TraceIdentifier, pdpDecision, businessLogicResult, null, deletedEntries?.OfType<DocumentEntryDto>());

        _atnaLogGeneratorService.CreateAuditLogForSoapRequestResponse(
            additionalParameters,
            requestSoapEnvelope,
            responseSoapEnvelope);

        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}