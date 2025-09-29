using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
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
public class XdsRespondingGatewayController : ControllerBase
{
    private readonly ILogger<XdsRespondingGatewayController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly XdsRegistryService _xdsRegistryService;
    private readonly XdsRepositoryService _xdsRepositoryService;
    private readonly IVariantFeatureManager _featureManager;

    public XdsRespondingGatewayController(ILogger<XdsRespondingGatewayController> logger, ApplicationConfig xdsConfig, XdsRegistryService xdsRegistryService, XdsRepositoryService xdsRepositoryService, IVariantFeatureManager featureManager, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _xdsRepositoryService = xdsRepositoryService;
        _featureManager = featureManager;
        _xdsRegistryService = xdsRegistryService;
        _httpClientFactory = httpClientFactory;
    }

    [Consumes("application/soap+xml", "application/xml", "multipart/related", "application/xop+xml")]
    [Produces("application/soap+xml", "application/xop+xml", "application/octet-stream", "multipart/related")]
    [HttpPost("RespondingGatewayService")]
    [UsePolicyEnforcementPoint]
    public async Task<IActionResult> HandleRespondingGatewayRequests([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action?.Trim();

        var responseEnvelope = new SoapEnvelope();
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Received request for action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");

        if (soapEnvelope.Header.ReplyTo?.Address != Constants.Soap.Addresses.Anonymous)
        {
            action += "Async";
        }

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti38ActionAsync:

                var iti38AsyncReplyTo = soapEnvelope.Header.ReplyTo?.Address;

                iti38AsyncReplyTo = iti38AsyncReplyTo.Replace("10.89.0.90", "pjd-ehs.test.nhn.no");
                
                if (string.IsNullOrEmpty(iti38AsyncReplyTo))
                    throw new InvalidOperationException("ReplyTo header is required for async ITI-39.");

                var iti38AsyncMessageId = soapEnvelope.Header.MessageId;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var response = await _xdsRegistryService.RegistryStoredQueryAsync(soapEnvelope);

                        var sxmls = new SoapXmlSerializer();
                        var soapXml = sxmls.SerializeSoapMessageToXmlString(response.Value).Content;

                        var soapXmlContent = new StringContent(soapXml, Encoding.UTF8, new MediaTypeHeaderValue(Constants.MimeTypes.SoapXml));

                        var client = _httpClientFactory.CreateClient();

                        var replyToResponse = await client.PostAsync(iti38AsyncReplyTo, soapXmlContent);

                        requestTimer.Stop();
                        _logger.LogInformation($"ReplyTo Endpoint status: {replyToResponse.StatusCode}");
                        _logger.LogInformation($"Completed async action: {action} in {requestTimer.ElapsedMilliseconds} ms");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Async exception thrown!\n{ex.ToString()}");

                        throw;
                    }
                });

                requestTimer.Stop();

                _logger.LogInformation($"Accepted async action: {action.Replace("Async", "")} in {requestTimer.ElapsedMilliseconds} ms");


                return Accepted();


            case Constants.Xds.OperationContract.Iti38Action:
                if (!await _featureManager.IsEnabledAsync("Iti38CrossGatewayQuery")) return NotFound();

                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);
                var iti38Response = await _xdsRegistryService.RegistryStoredQueryAsync(soapEnvelope);
                iti38Response.Value?.SetAction(Constants.Xds.OperationContract.Iti38Reply);

                responseEnvelope = iti38Response.Value;
                break;


            case Constants.Xds.OperationContract.Iti39ActionAsync:

                var iti39AyncReplyTo = soapEnvelope.Header.ReplyTo?.Address;

                iti38AsyncReplyTo = iti39AyncReplyTo.Replace("10.89.0.90","pjd-ehs.test.nhn.no");


                if (string.IsNullOrEmpty(iti39AyncReplyTo))
                    throw new InvalidOperationException("ReplyTo header is required for async ITI-39.");

                var iti39AsyncMessageId = Request.HttpContext.TraceIdentifier;

                _ = Task.Run(async () =>
                {
                    try
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


                        var bytes = await multipartContent.ReadAsByteArrayAsync();
                        var content = new ByteArrayContent(bytes);
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                            $"multipart/related; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\""
                        );

                        var client = _httpClientFactory.CreateClient();

                        var replyToResponse = await client.PostAsync(iti39AyncReplyTo, content);

                        requestTimer.Stop();
                        _logger.LogInformation($"{iti39AsyncMessageId} - ReplyTo Endpoint status: {replyToResponse.StatusCode}");
                        _logger.LogInformation($"{iti39AsyncMessageId} - Completed async action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"{iti39AsyncMessageId} - Async exception thrown!\n{ex.ToString()}");

                        throw;
                    }
                });

                requestTimer.Stop();

                _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Accepted async action: {action.Replace("Async", "")} in {requestTimer.ElapsedMilliseconds} ms");


                return Accepted();


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

                if (_xdsConfig.MultipartResponseForIti43AndIti39 is true)
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

                    _logger.LogInformation($"{soapEnvelope.Header.MessageId} - " + streamResult.ContentType);

                    _logger.LogInformation($"{soapEnvelope.Header.MessageId} - " + Encoding.UTF8.GetString(bytes));

                    return streamResult;
                }

                responseEnvelope = iti39Response.Value;
                break;

            default:
                _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Unknown action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");
                requestTimer.Stop();
                _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                return BadRequest(SoapExtensions.CreateSoapFault("soapenv:Reciever", detail: action, faultReason: $"The [action] cannot be processed at the receiver").Value);
        }

        requestTimer.Stop();
        _logger.LogInformation($"{soapEnvelope.Header.MessageId} -  Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");

        return Ok(responseEnvelope);
    }

    [HttpPost("RespondingGatewayService/replyto")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> FakeReplyToEndpoint([FromBody] SoapEnvelope soapEnvelope)
    {
        return Ok("Replied to");
    }
}