using Efferent.HL7.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[Tags("HL7 Endpoints")]
[ApiController]
[Route("hl7")]
public class Hl7MessagingController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private Hl7RegistryService _hl7RegistryService;
    private readonly IVariantFeatureManager _featureManager;


    public Hl7MessagingController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, Hl7RegistryService registryService, IVariantFeatureManager featureManager)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _hl7RegistryService = registryService;
        _featureManager = featureManager;
    }

    [Consumes("application/hl7-v2")]
    [Produces("application/hl7-v2")]
    [HttpPost("search-patients")]
    public async Task<IActionResult> SearchPatient([FromBody] Message hl7Message)
    {
        if (!await _featureManager.IsEnabledAsync("Hl7PatientQuery")) return NotFound();

        var responseMessage = new Message();

        switch (hl7Message.MessageStructure)
        {
            case "QBP_Q22":
            case "QBP_Q21":
                responseMessage = _hl7RegistryService.PatientDemographicsQueryGetPatientIdentifiersInRegistry(hl7Message);
                break;

            default:
                break;
        }
        var hl7String = responseMessage.SerializeMessage();
        return Content(hl7String, "application/hl7-v2");
    }
}