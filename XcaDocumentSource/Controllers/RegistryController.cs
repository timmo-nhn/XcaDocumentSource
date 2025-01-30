using Microsoft.AspNetCore.Mvc;
using XcaGatewayService.Commons;
using XcaGatewayService.Extensions;
using XcaGatewayService.Models.Soap;
using XcaGatewayService.Services;
using XcaGatewayService.Xca;

namespace XcaGatewayService.Controllers;

[ApiController]
[Route("Registry/services")]
public class RegistryController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;
    private readonly RegistryService _registryService;

    public RegistryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, RegistryService registryService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _registryService = registryService;
    }

    [Consumes("application/soap+xml")]
    [HttpPost("RegistryService")]
    public async Task<IActionResult> RegistryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;
        var registryResponse = new RegistryResponseType();
        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
                _registryService.RegistryStoredQuery(soapEnvelope);
                break;

            case Constants.Xds.OperationContract.Iti42Action:

                var registryUploadResponse = _registryService.AppendToRegistry(soapEnvelope);
               
                if (registryUploadResponse.IsSuccess)
                {
                    _logger.LogInformation("Registry updated successfully");
                    registryResponse.AddSuccess("Registry updated successfully");
                    return Ok(SoapExtensions.CreateSoapRegistryResponse(registryResponse));
                }
                else
                {
                    _logger.LogError("Error while updating registry", registryUploadResponse.Value.Body.Fault);
                }
                break;

            default:
                return Ok(SoapExtensions.CreateSoapFault("Missing action in SOAP <header> element","XDSMissingAction").Value);
        }
        return BadRequest();
    }
}
