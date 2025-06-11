using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;


namespace XcaXds.WebService.Controllers;

[ApiController]
[UsePolicyEnforcementPoint]
[Route("api/rest")]
[Consumes("application/json")]
[Produces("application/json")]
public class RestfulRegistryRepositoryController : ControllerBase
{
    private readonly ILogger<RestfulRegistryRepositoryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;

    public RestfulRegistryRepositoryController(ILogger<RestfulRegistryRepositoryController> logger, HttpClient httpClient, RestfulRegistryRepositoryService registryRestfulService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _restfulRegistryService = registryRestfulService;
    }

    [HttpGet("document-list")]
    public async Task<IActionResult> GetDocumentList([FromQuery] string? id, [FromQuery] string? status)
    {
        var entries = _restfulRegistryService.GetDocumentListForPatient(id, status);

        if (entries.Success)
        {
            return Ok(entries);
        }

        return BadRequest(entries);
    }

    [HttpGet("document")]
    public async Task<IActionResult> GetDocument([FromQuery] string? home, [FromQuery] string? repository, [FromQuery] string? document)
    {
        var entries = _restfulRegistryService.GetDocument(home, repository, document);

        if (entries.Success)
        {
            return Ok(entries);
        }

        return BadRequest(entries);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromBody] DocumentReferenceDto value)
    {
        _restfulRegistryService.UploadDocumentAndMetadata(value);
        return Ok("ok");
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateDocument(string id, [FromBody] DocumentReferenceDto value)
    {
        _restfulRegistryService.UpdateDocumentMetadata(value);

        return Ok("ok");
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteDocument(string id)
    {
        _restfulRegistryService.DeleteDocumentAndMetadata(id);
        return Ok("ok");
    }
}
