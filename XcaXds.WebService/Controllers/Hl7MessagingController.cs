using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Hl7;
using XcaXds.Commons.Models.Hl7.Message;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("hl7")]
public class Hl7MessagingController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly XdsConfig _xdsConfig;
    private RegistryService _registryService;

    public Hl7MessagingController(ILogger<RegistryController> logger, XdsConfig xdsConfig, RegistryService registryService)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryService = registryService;
    }

    [Consumes("application/hl7-v2")]
    [Produces("application/hl7-v2")]
    [HttpPost("search-patients")]
    public async Task<IActionResult> SearchPatient([FromBody]Hl7RawMessage hl7Message)
    {
        var pdqMessage = new QBP_Q22();
        _registryService.PatientDemographicsQueryGetPatientIdentifiersInRegistry(pdqMessage);
        return Ok();
    }
}