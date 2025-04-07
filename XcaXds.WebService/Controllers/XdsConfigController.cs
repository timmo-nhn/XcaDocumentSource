using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Xca;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class XdsConfigController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly XdsConfig _xdsConfig;

    public XdsConfigController(ILogger<RegistryController> logger, XdsConfig xdsConfig)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
    }

    [Produces("application/json")]
    [HttpGet("about")]
    public async Task<IActionResult> HandlePostRequest()
    {
        return Ok(_xdsConfig);
    }
}