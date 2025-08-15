using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class PolicyManagementController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;

    public PolicyManagementController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
    }

    [Produces("application/json")]
    [HttpGet("policy/getall")]
    public async Task<IActionResult> HandleRequest(bool asXml = false)
    {


        return Ok();
    }
}