using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Models.Soap.Actions;
using XcaDocumentSource.Models.Soap.Xds;
using XcaDocumentSource.Services;
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
    public async Task<IActionResult> CrossGatewayQuery([FromBody] SoapEnvelope xmlBody)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        switch (xmlBody.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti38Action:
                var response = await _xcaGateway.RegistryStoredQuery(xmlBody, baseUrl + "/Registry/services/RegistryService");
                break;

            default:
                return BadRequest("Missing action in SOAP <header> element");
        }

        return Ok($"Response from Responding Gateway: ");
    }

    private SoapEnvelope<T>? ConvertToTypedEnvelope<T>(SoapEnvelope request)
    {

        /*if
        var newSoap = new SoapEnvelope<T>()
        {
            Header = request.Header,
            Body = (T)request.Body
        };
        return newSoap;*/
        return null; 
    }

}
