using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
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
    private readonly RestfulRegistryService _registryRestfulService;

    public RegistryRestfulController(ILogger<RegistryRestfulController> logger, HttpClient httpClient, RestfulRegistryService registryRestfulService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _registryRestfulService = registryRestfulService;
    }

    [HttpGet("document-list")]
    public async Task<IActionResult> GetDocumentList([FromQuery] string? id, [FromQuery] string? status)
    {

        var entries = _registryRestfulService.GetDocumentListForPatient(id, status);
        
        return Ok(entries);
    }

    [HttpGet("document")]
    public async Task<IActionResult> GetDocument([FromQuery] string? homecommunity_id, [FromQuery] string? repository_id, [FromQuery] string? document_id)
    {
        if (string.IsNullOrWhiteSpace(homecommunity_id) || string.IsNullOrWhiteSpace(repository_id) || string.IsNullOrWhiteSpace(document_id))
            return BadRequest("Parameters homecommunity_id, repository_id, document_id are required.");

        var entries = _registryRestfulService.GetDocument(homecommunity_id, repository_id, document_id);
        
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
