using Microsoft.AspNetCore.Mvc;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class ApplicationMetadataController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;

    public ApplicationMetadataController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
    }

    [Produces("application/json")]
    [HttpGet("about/config")]
    public async Task<IActionResult> GetXdsConfig()
    {
        return Ok(_xdsConfig);
    }
}