using Microsoft.AspNetCore.Mvc;
using XcaXds.Shared.Extensions;
using XcaXds.Terminology.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api/terminology")]
public class TerminologyController : ControllerBase
{
    private readonly TerminologyService _terminologyService;
    private readonly ILogger<TerminologyController> _logger;

    public TerminologyController(TerminologyService terminologyService, ILogger<TerminologyController> logger)
    {
        _terminologyService = terminologyService;
        _logger = logger;
    }

    [Produces("application/json")]
    [HttpGet("{codeSystemName}")]
    public IActionResult GetCodeSystemByName(string codeSystemName)
    {
        try
        {
            var codeSystems = _terminologyService.GetCodeSystemByKey(codeSystemName);
            return Ok(codeSystems);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogInformation("Code system '{codeSystemName}' was not found", codeSystemName);
            return NotFound();
        }
    }

    [Produces("application/json")]
    [HttpGet("{codeSystemName}/values")]
    public IActionResult GetCodeSystemValues(string codeSystemName)
    {
        try
        {
            var values = _terminologyService.GetCodeSystemByKey(codeSystemName).Values();
            return values is { Length: > 0 } ? Ok(values) : NotFound();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogInformation("Code system '{codeSystemName}' was not found", codeSystemName);
            return NotFound();
        }
    }

    [Produces("application/json")]
    [HttpGet("{codeSystemName}/values/by-name")]
    public IActionResult GetCodeSystemValuesByName(string codeSystemName, [FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Query parameter 'name' is required.");
        }

        var values = _terminologyService.GetValueFromCodeSystemByName(codeSystemName, name);
        return values is { Length: > 0 } ? Ok(values) : NotFound();
    }
}
