using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Services;
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
    public async Task<IActionResult> RespondingGatewayService([FromBody] SoapEnvelope xmlBody)
    {

        return Ok($"Response from Responding Gateway: ");
    }
}
