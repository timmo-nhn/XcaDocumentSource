using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Xca;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("XCA/services")]
public class RespondingGatewayController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;

    public RespondingGatewayController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
    }

    [Consumes("application/soap+xml")]
    [Produces("application/soap+xml")]
    [HttpPost("RespondingGatewayService")]
    public async Task<IActionResult> CrossGatewayQuery([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action.Trim();
        switch (action)
        {
            case Constants.Xds.OperationContract.Iti38Action:
                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);
                var response = await _xcaGateway.RegistryStoredQuery(soapEnvelope, baseUrl + "/Registry/services/RegistryService");
                return Ok(response);

            default:
                if (action != null)
                {
                    _logger.LogError($"Unknown action {action}");
                    return Ok(SoapExtensions.CreateSoapFault("UnknownAction", $"Unknown action {action}"));
                }
                else
                {
                    _logger.LogError($"Missing action {action}");
                    return Ok(SoapExtensions.CreateSoapFault("MissingAction", $"Missing action {action}"));
                }
        }
    }
}
