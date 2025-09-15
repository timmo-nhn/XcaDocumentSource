using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Net;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
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
    private readonly XdsRegistryService _xdsRegistryService;
    private readonly XdsRepositoryService _xdsRepositoryService;
    private readonly IVariantFeatureManager _featureManager;

    public XdsRespondingGatewayController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, XdsRegistryService xdsRegistryService, XdsRepositoryService xdsRepositoryService, IVariantFeatureManager featureManager)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
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

                break;


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

                    var contentResult = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = await responseMessage.Content.ReadAsStringAsync(),
                        ContentType = Constants.MimeTypes.MultipartRelated
                    };

                    if (contentId != null)
                    {
                        contentResult.ContentType += $"; boundary=\"{boundary}\"; start=\"{contentId}\"";
                    }

                    _logger.LogInformation(contentResult.Content);
                    _logger.LogInformation(contentResult.ContentType);
                    
                    return contentResult;
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