using Microsoft.AspNetCore.Mvc;
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Xca;

namespace XcaDocumentSource.Controllers;

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
    [HttpPost("RespondingGatewayService")]
    public async Task<IActionResult> CrossGatewayQuery([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;
        switch (action)
        {
            case Constants.Xds.OperationContract.Iti38Action:
                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.Header.Action = Constants.Xds.OperationContract.Iti18Action;
                var response = await _xcaGateway.RegistryStoredQuery(soapEnvelope, baseUrl + "/Registry/services/RegistryService");
                return Ok(response);

            default:
                if (action != null)
                {

                    return Ok(soapEnvelope.CreateSoapFault("UnknownAction", $"Unknown action {action}"));
                }
                else
                {
                    return Ok(soapEnvelope.CreateSoapFault("MissingAction", $"Missing action {action}".Trim(), "fix action m8"));
                }
        }
    }
}
