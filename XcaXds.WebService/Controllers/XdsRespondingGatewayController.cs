using Abc.Xacml.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services;

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
    private readonly MonitoringStatusService _monitoringService;

    private static readonly ActivitySource ActivitySource = new("nhn.xcads");
    private static readonly Meter Meter = new("nhn.Xcads.RespondingGateway", "1.0.0");

    private static readonly Counter<int> QueryCounter =
        Meter.CreateCounter<int>("RespondingGateway.Query.count", description: "Requests to Query from registry or repository");
    private static readonly Counter<int> RetrieveCounter =
        Meter.CreateCounter<int>("RespondingGateway.Retrieve.count", description: "Requests to Retrieve from registry or repository");


    public XdsRespondingGatewayController(
        ILogger<XdsRespondingGatewayController> logger,
        ApplicationConfig xdsConfig,
        XdsRegistryService xdsRegistryService,
        XdsRepositoryService xdsRepositoryService,
        IVariantFeatureManager featureManager,
        IHttpClientFactory httpClientFactory,
        MonitoringStatusService monitoringService
        )
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _xdsRepositoryService = xdsRepositoryService;
        _featureManager = featureManager;
        _xdsRegistryService = xdsRegistryService;
        _httpClientFactory = httpClientFactory;
        _monitoringService = monitoringService;
    }

    [Consumes("application/soap+xml", "application/xml", "multipart/related", "application/xop+xml")]
    [Produces("application/soap+xml", "application/xop+xml", "application/octet-stream", "multipart/related")]
    [HttpPost("RespondingGatewayService")]
    [ExportsAtnaAuditLog]
    [UsePolicyEnforcementPoint]
    public async Task<IActionResult> HandleRespondingGatewayRequests([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action?.Trim();

        using var activity = ActivitySource.StartActivity($"RespondingGatewayService");
        activity?.SetTag("Request.Action", action);
        activity?.SetTag("Request.SessionId", soapEnvelope.Header.MessageId);

        var responseEnvelope = new SoapEnvelope();
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Received request for action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");

        XacmlContextRequest? xacmlRequest = null;

        if (HttpContext.Items.TryGetValue("xacmlRequest", out var xamlContextRequestObject) && xamlContextRequestObject is XacmlContextRequest xacmlContextRequest)
        {
            xacmlRequest = xacmlContextRequest;
        }

        if (soapEnvelope.Header.ReplyTo?.Address != Constants.Soap.Addresses.Anonymous)
        {
            action += "Async";
        }

        FileContentResult? multipartResponse = null;

        switch (action)
        {

            case Constants.Xds.OperationContract.Iti38Action:
                if (!await _featureManager.IsEnabledAsync("Iti38CrossGatewayQuery")) return NotFound();

                QueryCounter.Add(1);

                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);
                var iti38Response = _xdsRegistryService.RegistryStoredQuery(soapEnvelope, xacmlRequest);
                iti38Response.Value?.SetAction(Constants.Xds.OperationContract.Iti38Reply);
                responseEnvelope = iti38Response.Value;
                break;


            case Constants.Xds.OperationContract.Iti39Action:
                if (!await _featureManager.IsEnabledAsync("Iti39CrossGatewayRetrieve")) return NotFound();

                RetrieveCounter.Add(1);

                // Only change from ITI-39 to ITI-43 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti43Action);
                var iti39Response = _xdsRepositoryService.RetrieveDocumentSet(soapEnvelope, xacmlRequest);
                iti39Response.Value?.SetAction(Constants.Xds.OperationContract.Iti39Reply);



                if (iti39Response.IsSuccess is false)
                {
                    responseEnvelope = iti39Response.Value;
                    break;
                }

                if (_xdsConfig.MultipartResponseForIti43AndIti39 is true && Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated && iti39Response.Value != null)
                {
                    var multipartContent = MultipartExtensions.ConvertRetrieveDocumentSetResponseToMultipartResponse(iti39Response.Value, out var boundary);

                    string? contentId = null;

                    if (multipartContent.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var contentIdValues) ?? false)
                    {
                        contentId = contentIdValues.First();
                    }

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = multipartContent
                    };

                    requestTimer.Stop();
                    _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                    _monitoringService.ResponseTimes.Add(action, requestTimer.ElapsedMilliseconds);

                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                    multipartResponse = new FileContentResult(bytes, $"multipart/related; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"");

                    _logger.LogInformation($"{soapEnvelope.Header.MessageId} - " + multipartResponse.ContentType);

                    _logger.LogInformation($"{soapEnvelope.Header.MessageId} - " + Encoding.UTF8.GetString(bytes));
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

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Exporting AuditEvent for {action} request");

        _monitoringService.ResponseTimes.Add(action, requestTimer.ElapsedMilliseconds);

        // multipart RetrieveDocumentSet needs to be returned as its own object
        if (multipartResponse != null)
            return multipartResponse;

        return Ok(responseEnvelope);
    }

    [HttpPost("RespondingGatewayService/replyto")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> FakeReplyToEndpoint([FromBody] SoapEnvelope soapEnvelope)
    {
        return Ok("Replied to");
    }
}