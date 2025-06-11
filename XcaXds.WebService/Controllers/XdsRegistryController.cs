using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using XcaXds.Commons;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("Registry/services")]
[UsePolicyEnforcementPoint]
public class XdsRegistryController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;
    private readonly XdsRegistryService _registryService;
    private readonly IVariantFeatureManager _featureManager;

    public XdsRegistryController(ILogger<XdsRegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, XdsRegistryService registryService, IVariantFeatureManager featureManager)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _registryService = registryService;
        _featureManager = featureManager;
    }

    [Consumes("application/soap+xml")]
    [Produces("application/soap+xml")]
    [HttpPost("RegistryService")]
    public async Task<IActionResult> RegistryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();
        var xmlSerializer = new SoapXmlSerializer(XmlSettings.Soap);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;

        var responseEnvelope = new SoapEnvelope();
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"Received request for action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
                if (!await _featureManager.IsEnabledAsync("Iti18RegistryStoredQuery")) return NotFound();

                var registryQueryResponse = await _registryService.RegistryStoredQueryAsync(soapEnvelope);

                responseEnvelope = registryQueryResponse.Value;
                responseEnvelope.Header = new()
                {
                    Action = soapEnvelope.GetCorrespondingResponseAction(),
                    Security = null,
                    RelatesTo = soapEnvelope.Header.MessageId
                };

                break;

            case Constants.Xds.OperationContract.Iti42Action:
                if (!await _featureManager.IsEnabledAsync("Iti42RegisterDocumentSet")) return NotFound();

                var registryUploadResponse = await _registryService.AppendToRegistryAsync(soapEnvelope);

                if (registryUploadResponse.IsSuccess)
                {
                    _logger.LogInformation("Registry updated successfully");
                    registryResponse.EvaluateStatusCode();

                    responseEnvelope.Header = new()
                    {
                        Action = soapEnvelope.GetCorrespondingResponseAction(),
                        Security = null,
                        RelatesTo = soapEnvelope.Header.MessageId
                    };
                    responseEnvelope.Body = new()
                    {
                        RegistryResponse = registryResponse
                    };
                }
                else
                {
                    _logger.LogError("Error while updating registry", registryUploadResponse.Value?.Body.Fault);
                    registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Error while updating registry", "XDS Registry");

                    registryResponse.RegistryErrorList.RegistryError = [.. registryResponse.RegistryErrorList.RegistryError, .. registryUploadResponse.Value?.Body.RegistryResponse?.RegistryErrorList.RegistryError];

                    responseEnvelope = SoapExtensions.CreateSoapResultRegistryResponse(registryResponse).Value;
                }
                break;

            case Constants.Xds.OperationContract.Iti62Action:
                if (!await _featureManager.IsEnabledAsync("Iti62DeleteDocumentSet")) return NotFound();

                var deleteDocumentSetResponse = await _registryService.DeleteDocumentSetAsync(soapEnvelope);
                if (deleteDocumentSetResponse.IsSuccess)
                {
                    responseEnvelope.Header ??= new();
                    responseEnvelope.Header.Action = soapEnvelope.GetCorrespondingResponseAction();
                    responseEnvelope.Body = new();
                    responseEnvelope.Body = deleteDocumentSetResponse.Value.Body;
                }
                break;

            default:
                _logger.LogInformation($"Unknown action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");
                requestTimer.Stop();
                _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                return BadRequest(SoapExtensions.CreateSoapFault("soapenv:Reciever", detail: action, faultReason: $"The [action] cannot be processed at the receiver").Value);
        }
        requestTimer.Stop();
        _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
        registryResponse.EvaluateStatusCode();
        return Ok(responseEnvelope);
    }
}
