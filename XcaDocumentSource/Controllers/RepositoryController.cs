using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Services;
using XcaDocumentSource.Xca;

namespace XcaDocumentSource.Controllers;

[ApiController]
[Route("Repository/services")]
public class RepositoryController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;

    public RepositoryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
    }

    [Consumes("application/soap+xml")]
    [HttpPost("RepositoryService")]
    public async Task<IActionResult> RespondingGatewayService([FromBody] SoapEnvelope xmlBody)
    {
        switch (xmlBody.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti43Action:
                // Hent dokument!!!
                break;

            default:
                return BadRequest("Unknown action in SOAP <header> element");
        }

        return Ok($"Response from Responding Gateway: ");
    }
}
