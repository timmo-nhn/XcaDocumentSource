using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Net;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Xca;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("XCA/services")]
[UsePolicyEnforcementPoint]
public class XdsRespondingGatewayController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;
    private readonly ApplicationConfig _xdsConfig;
    private readonly XdsRepositoryService _repositoryService;
    private readonly IVariantFeatureManager _featureManager;

    public XdsRespondingGatewayController(ILogger<XdsRegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, ApplicationConfig xdsConfig, XdsRepositoryService repositoryService, IVariantFeatureManager featureManager)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _xdsConfig = xdsConfig;
        _repositoryService = repositoryService;
        _featureManager = featureManager;
    }

    [Consumes("application/soap+xml")]
    [Produces("application/soap+xml", "application/xop+xml", "application/octet-stream", "multipart/related")]
    [HttpPost("RespondingGatewayService")]
    public async Task<IActionResult> CrossGatewayQuery([FromBody] SoapEnvelope soapEnvelope)
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


                var iti38AsyncResponse = await _xcaGateway.RegistryStoredQuery(soapEnvelope, baseUrl + "/Registry/services/RegistryService");
                iti38AsyncResponse.Value.SetAction(Constants.Xds.OperationContract.Iti38ReplyAsync);

                return Accepted(SoapExtensions.CreateAsyncAcceptedMessage(soapEnvelope));


            case Constants.Xds.OperationContract.Iti38Action:
                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);
                var iti38Response = await _xcaGateway.RegistryStoredQuery(soapEnvelope, baseUrl + "/Registry/services/RegistryService");
                iti38Response.Value.SetAction(Constants.Xds.OperationContract.Iti38Reply);

                responseEnvelope = iti38Response.Value;
                break;




            case Constants.Xds.OperationContract.Iti39ActionAsync:

                break;


            case Constants.Xds.OperationContract.Iti39Action:
                // Only change from ITI-39 to ITI-43 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti43Action);
                var iti39Response = await _xcaGateway.RetrieveDocumentSet(soapEnvelope, baseUrl + "/Repository/services/RepositoryService");
                iti39Response.Value?.SetAction(Constants.Xds.OperationContract.Iti39Reply);

                if (iti39Response.IsSuccess is false)
                {
                    responseEnvelope = iti39Response.Value;
                    break;
                }

                if (_xdsConfig.MultipartResponseForIti43 is true)
                {
                    var multipartContent = _repositoryService.ConvertToMultipartResponse(iti39Response.Value);

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = multipartContent
                    };

                    return new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = responseMessage.Content.ReadAsStringAsync().Result,
                        ContentType = Constants.MimeTypes.MultipartRelated
                    };
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