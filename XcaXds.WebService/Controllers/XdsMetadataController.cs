using Microsoft.AspNetCore.Mvc;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class XdsMetadataController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly XdsConfig _xdsConfig;

    public XdsMetadataController(ILogger<RegistryController> logger, XdsConfig xdsConfig)
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