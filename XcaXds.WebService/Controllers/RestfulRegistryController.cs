using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;

[ApiController]
[UsePolicyEnforcementPoint]
[Route("api/[controller]")]
public class RestfulRegistryController : ControllerBase
{
    private readonly ILogger<RestfulRegistryController> _logger;
    private readonly HttpClient _httpClient;

    public RestfulRegistryController(ILogger<RestfulRegistryController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    [HttpPost]
    public void Post([FromBody] RegistryObjectDto value)
    {
    }

    [HttpPut]
    public void Put([FromBody] RegistryObjectDto value)
    {
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
