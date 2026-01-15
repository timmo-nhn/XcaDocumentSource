using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Controllers;

[Tags("SOAP Endpoints (IHE XDS/XCA)")]
[ApiController]
[Route("Registry/services")]
[UsePolicyEnforcementPoint]
public class XdsRegistryController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly XdsRegistryService _registryService;
    private readonly IVariantFeatureManager _featureManager;
    private readonly AuditLogGeneratorService _auditLoggingService;

    public XdsRegistryController(
        ILogger<XdsRegistryController> logger,
        XdsRegistryService registryService,
        IVariantFeatureManager featureManager,
        AuditLogGeneratorService auditLoggingService
        )
    {
        _logger = logger;
        _registryService = registryService;
        _featureManager = featureManager;
        _auditLoggingService = auditLoggingService;
    }

    [Consumes("application/soap+xml", "application/xml", "multipart/related")]
    [Produces("application/soap+xml", "application/xml")]
    [HttpPost("RegistryService")]
    public async Task<IActionResult> RegistryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;

        var responseEnvelope = new SoapEnvelope();
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Received request for action: {action}");

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
                if (!await _featureManager.IsEnabledAsync("Iti18RegistryStoredQuery")) return NotFound();

                if (soapEnvelope.Body.AdhocQueryRequest == null)
                {
                    responseEnvelope = SoapExtensions.CreateSoapFault("soapenv:Client", "ITI-18 Request does not contain a valid Query Request").Value;
                    break;
                }

                var registryQueryResponse = _registryService.RegistryStoredQueryAsync(soapEnvelope);

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

                if (soapEnvelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList == null || soapEnvelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList.Length == 0)
                {
                    responseEnvelope = SoapExtensions.CreateSoapFault("soapenv:Client", "ITI-42 Request does not contain any RegistryObjects to upload").Value;
                    break;
                }

                var registryUploadResponse = _registryService.AppendToRegistryAsync(soapEnvelope);

                if (registryUploadResponse.IsSuccess)
                {
                    _logger.LogInformation("Registry updated successfully");

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
                    _logger.LogError($"{Request.HttpContext.TraceIdentifier} - Error while updating registry", registryUploadResponse.Value?.Body.Fault);
                    registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Error while updating registry", "XDS Registry");

                    registryResponse.RegistryErrorList ??= new();
                    registryResponse.RegistryErrorList.RegistryError = [.. registryResponse.RegistryErrorList.RegistryError, .. registryUploadResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError];

                    responseEnvelope = SoapExtensions.CreateSoapResultRegistryResponse(registryResponse).Value;
                }
                break;

            case Constants.Xds.OperationContract.Iti62Action:
                if (!await _featureManager.IsEnabledAsync("Iti62DeleteDocumentSet")) return NotFound();

                if (soapEnvelope.Body.RemoveObjectsRequest?.ObjectRefList?.ObjectRef == null || soapEnvelope.Body.RemoveObjectsRequest?.ObjectRefList?.ObjectRef.Length == 0)
                {
                    responseEnvelope = SoapExtensions.CreateSoapFault("soapenv:Client", "ITI-62 Request does not specify any objects to delete").Value;
                    break;
                }

                var deleteDocumentSetResponse = _registryService.DeleteDocumentSetAsync(soapEnvelope);
                if (deleteDocumentSetResponse.IsSuccess)
                {
                    responseEnvelope.Header ??= new();
                    responseEnvelope.Header.Action = soapEnvelope.GetCorrespondingResponseAction();
                    responseEnvelope.Body = new();
                    responseEnvelope.Body = deleteDocumentSetResponse.Value?.Body;
                }

                break;

            default:
                _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Unknown action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");
                requestTimer.Stop();
                _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                return BadRequest(SoapExtensions.CreateSoapFault("soapenv:Reciever", detail: action, faultReason: $"The [action] cannot be processed at the receiver").Value);
        }

        _auditLoggingService.CreateAuditLogForSoapRequestResponse(soapEnvelope, responseEnvelope);

        requestTimer.Stop();
        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
        registryResponse.EvaluateStatusCode();
        return Ok(responseEnvelope);
    }
}
