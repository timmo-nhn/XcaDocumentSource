using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Shared;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Controllers;

[Tags("SOAP Endpoints (IHE XDS/XCA)")]
[ApiController]
[Route("XCA/services")]
[ExportsStatistics]
public class XdsRespondingGatewayController : ControllerBase
{
    private readonly ILogger<XdsRespondingGatewayController> _logger;
    private readonly IVariantFeatureManager _featureManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationConfig _xdsConfig;
    private readonly DocumentListFiltererService _documentListFiltererService;
    private readonly XdsRegistryService _xdsRegistryService;
    private readonly XdsRepositoryService _xdsRepositoryService;
    private readonly MonitoringStatusService _monitoringService;

    private static readonly ActivitySource ActivitySource = new("nhn.xcads");
    private static readonly Meter Meter = new("nhn.Xcads.RespondingGateway", "1.0.0");

    private static readonly Counter<int> QueryCounter = Meter.CreateCounter<int>("RespondingGateway.Query.count", description: "Requests to Query from registry or repository");
    private static readonly Counter<int> RetrieveCounter = Meter.CreateCounter<int>("RespondingGateway.Retrieve.count", description: "Requests to Retrieve from registry or repository");


    public XdsRespondingGatewayController(
        ILogger<XdsRespondingGatewayController> logger,
        IVariantFeatureManager featureManager,
        IHttpClientFactory httpClientFactory,
        DocumentListFiltererService documentListFiltererService,
        ApplicationConfig xdsConfig,
        XdsRegistryService xdsRegistryService,
        XdsRepositoryService xdsRepositoryService,
        MonitoringStatusService monitoringService)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _xdsRepositoryService = xdsRepositoryService;
        _documentListFiltererService = documentListFiltererService;
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
        _logger.LogInformation("{traceIdentifier} - Received request for action: {action} from {remoteIpAddress}", soapEnvelope.Header.MessageId, action, Request.HttpContext.Connection.RemoteIpAddress);

        var accessControlRequest = HttpContext.Items.TryGetValue("accessRequest", out var arqst) ? arqst as AbacRequest : null;

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

                var iti38Response = _xdsRegistryService.RegistryStoredQuery(soapEnvelope);
                var filteredDocumentList = _documentListFiltererService.FilterAdhocQueryResponseBasedOnBusinessLogic(soapEnvelope, iti38Response.Value, accessControlRequest, out var businessLogicResults);
                HttpContext.Items.Add("businessLogicResult", businessLogicResults);
                iti38Response.Value?.SetAction(Constants.Xds.OperationContract.Iti38Reply);
                responseEnvelope = filteredDocumentList;
                break;


            case Constants.Xds.OperationContract.Iti39Action:
                if (!await _featureManager.IsEnabledAsync("Iti39CrossGatewayRetrieve")) return NotFound();

                RetrieveCounter.Add(1);

                // Only change from ITI-39 to ITI-43 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti43Action);
                var iti39Response = _xdsRepositoryService.RetrieveDocumentSet(soapEnvelope, accessControlRequest);
                iti39Response.Value?.SetAction(Constants.Xds.OperationContract.Iti39Reply);



                if (iti39Response.IsSuccess == false)
                {
                    responseEnvelope = iti39Response.Value;
                    break;
                }

                if (_xdsConfig.MultipartResponseForIti43AndIti39 == true && Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated && iti39Response.Value != null)
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
                    _logger.LogInformation("{traceIdentifier} - Completed action: {action} in {elapsedMilliseconds} ms", soapEnvelope.Header.MessageId, action, requestTimer.ElapsedMilliseconds);
                    _monitoringService.ResponseTimes.Add(action, requestTimer.ElapsedMilliseconds);

                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                    multipartResponse = new FileContentResult(bytes, $"multipart/related; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"");

                    _logger.LogInformation("{traceIdentifier} - {contentType}", soapEnvelope.Header.MessageId, multipartResponse.ContentType);

                    _logger.LogInformation("{traceIdentifier} - {content}", soapEnvelope.Header.MessageId, Encoding.UTF8.GetString(bytes));
                }

                responseEnvelope = iti39Response.Value;
                break;

            default:
                _logger.LogInformation("{traceIdentifier} - Unknown action: {action} from {remoteIpAddress}", soapEnvelope.Header.MessageId, action, Request.HttpContext.Connection.RemoteIpAddress);
                requestTimer.Stop();
                _logger.LogInformation("{traceIdentifier} - Completed action: {action} in {elapsedMilliseconds} ms", soapEnvelope.Header.MessageId, action, requestTimer.ElapsedMilliseconds);
                return BadRequest(SoapExtensions.CreateSoapFault("soapenv:Reciever", detail: action, faultReason: $"The [action] cannot be processed at the receiver").Value);
        }

        requestTimer.Stop();
        _logger.LogInformation("{traceIdentifier} -  Completed action: {action} in {elapsedMilliseconds} ms", soapEnvelope.Header.MessageId, action, requestTimer.ElapsedMilliseconds);

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