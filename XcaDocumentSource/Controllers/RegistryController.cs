using Microsoft.AspNetCore.Mvc;
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Xca;

namespace XcaDocumentSource.Controllers;

[ApiController]
[Route("Registry/services")]
public class RegistryController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;

    public RegistryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
    }

    [Consumes("application/soap+xml")]
    [HttpPost("RegistryService")]
    public async Task<IActionResult> RegistryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;
        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
                var b = ";";
                break;

            default:
                return BadRequest("Missing action in SOAP <header> element");
        }
        return BadRequest("Missing action in SOAP <header> element");
    }
}
