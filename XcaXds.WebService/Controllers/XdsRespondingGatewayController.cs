using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;

[Tags("SOAP Endpoints (IHE XDS/XCA)")]
[ApiController]
[Route("XCA/services")]
[UsePolicyEnforcementPoint]
public class XdsRespondingGatewayController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly HttpClient _httpClient;
    private readonly XdsRegistryService _xdsRegistryService;
    private readonly XdsRepositoryService _xdsRepositoryService;
    private readonly IVariantFeatureManager _featureManager;

    public XdsRespondingGatewayController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, XdsRegistryService xdsRegistryService, XdsRepositoryService xdsRepositoryService, IVariantFeatureManager featureManager, HttpClient httpClient)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _httpClient = httpClient;
        _xdsRepositoryService = xdsRepositoryService;
        _featureManager = featureManager;
        _xdsRegistryService = xdsRegistryService;
    }

    [Consumes("application/soap+xml", "application/xml", "multipart/related", "application/xop+xml")]
    [Produces("application/soap+xml", "application/xop+xml", "application/octet-stream", "multipart/related")]
    [HttpPost("RespondingGatewayService")]
    public async Task<IActionResult> HandleRespondingGatewayRequests([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action?.Trim();

        var responseEnvelope = new SoapEnvelope();
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"Received request for action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");

        if (soapEnvelope.Header.ReplyTo?.Address != Constants.Soap.Addresses.Anonymous)
        {
            action += "Async";
        }

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti38ActionAsync:

                break;

                var responseUrl = soapEnvelope.Header.ReplyTo?.Address;

                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);

                return Accepted(SoapExtensions.CreateAsyncAcceptedMessage(soapEnvelope));


            case Constants.Xds.OperationContract.Iti38Action:
                if (!await _featureManager.IsEnabledAsync("Iti38CrossGatewayQuery")) return NotFound();

                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);
                var iti38Response = await _xdsRegistryService.RegistryStoredQueryAsync(soapEnvelope);
                iti38Response.Value?.SetAction(Constants.Xds.OperationContract.Iti38Reply);

                responseEnvelope = iti38Response.Value;
                break;


            case Constants.Xds.OperationContract.Iti39ActionAsync:

                var replyTo = soapEnvelope.Header.ReplyTo?.Address?.ToString();

                if (string.IsNullOrEmpty(replyTo))
                    throw new InvalidOperationException("ReplyTo header is required for async ITI-39.");

                var messageId = soapEnvelope.Header.MessageId;

                _ = Task.Run(async () =>
                {

                    var response = await _xdsRepositoryService.RetrieveDocumentSet(soapEnvelope);

                    var sxmls = new SoapXmlSerializer();

                    var multipartContent = HttpRequestResponseExtensions.ConvertToMultipartResponse(response.Value, out var boundary);

                    string contentId = null;

                    if (multipartContent.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var contentIdValues) ?? false)
                    {
                        contentId = contentIdValues.First();
                    }

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = multipartContent
                    };

                    requestTimer.Stop();
                    _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");

                    var bytes = await multipartContent.ReadAsByteArrayAsync();
                    var content = new ByteArrayContent(bytes);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                        $"multipart/related; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\""
                    );

                });

                requestTimer.Stop();

                _logger.LogInformation($"Accepted async action: {action.Replace("Async","")} in {requestTimer.ElapsedMilliseconds} ms");


                return Accepted(SoapExtensions.CreateAsyncAcceptedMessage(soapEnvelope));


            case Constants.Xds.OperationContract.Iti39Action:
                if (!await _featureManager.IsEnabledAsync("Iti39CrossGatewayRetrieve")) return NotFound();

                // Only change from ITI-39 to ITI-43 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti43Action);
                var iti39Response = await _xdsRepositoryService.RetrieveDocumentSet(soapEnvelope);
                iti39Response.Value?.SetAction(Constants.Xds.OperationContract.Iti39Reply);

                if (iti39Response.IsSuccess is false)
                {
                    responseEnvelope = iti39Response.Value;
                    break;
                }

                if (_xdsConfig.MultipartResponseForIti43 is true)
                {
                    var multipartContent = HttpRequestResponseExtensions.ConvertToMultipartResponse(iti39Response.Value, out var boundary);

                    string contentId = null;

                    if (multipartContent.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var contentIdValues) ?? false)
                    {
                        contentId = contentIdValues.First();
                    }

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = multipartContent
                    };

                    requestTimer.Stop();
                    _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");

                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                    var streamResult = new FileContentResult(bytes, $"multipart/related; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"");

                    _logger.LogInformation(streamResult.ContentType);

                    _logger.LogInformation(Encoding.UTF8.GetString(bytes));

                    return streamResult;
                }

                responseEnvelope = iti39Response.Value;
                break;

            default:
                _logger.LogInformation($"Unknown action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");
                requestTimer.Stop();
                _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                return BadRequest(SoapExtensions.CreateSoapFault("soapenv:Reciever", detail: action, faultReason: $"The [action] cannot be processed at the receiver").Value);
        }

        requestTimer.Stop();
        _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");

        return Ok(responseEnvelope);
    }
}