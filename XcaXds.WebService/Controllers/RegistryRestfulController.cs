using Microsoft.AspNetCore.Mvc;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;


namespace XcaXds.WebService.Controllers;

[ApiController]
[UsePolicyEnforcementPoint]
[Route("api/rest")]
[Consumes("application/json")]
[Produces("application/json")]
public class RegistryRestfulController : ControllerBase
{
    private readonly ILogger<RegistryRestfulController> _logger;
    private readonly HttpClient _httpClient;
    private readonly RegistryRestfulService _registryRestfulService;

    public RegistryRestfulController(ILogger<RegistryRestfulController> logger, HttpClient httpClient, RegistryRestfulService registryRestfulService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _registryRestfulService = registryRestfulService;
    }

    [HttpGet("registry-objects")]
    public async Task<IActionResult> Get([FromQuery] string? id, [FromQuery] string? status)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(status))
            return BadRequest("Both 'id' and 'status' query parameters are required.");

        var allowedStatuses = new[] { "Approved", "Deprecated" };
        if (!allowedStatuses.Contains(status))
            return BadRequest("Status must be one of \"Approved\" or \"Deprecated\".");

        var entries = _registryRestfulService.GetDocumentListForPatient(id);

        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string value)
    {
        return Ok("ok");
    }

    // PUT api/<ValuesController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<ValuesController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
