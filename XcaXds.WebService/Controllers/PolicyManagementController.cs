using Microsoft.AspNetCore.Mvc;
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
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyManagementController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper, PolicyRepositoryWrapper policyRepositoryWrapper)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _policyRepositoryWrapper = policyRepositoryWrapper;
    }

    [Produces("application/json")]
    [HttpGet("policy/getall")]
    public async Task<IActionResult> GetAllPolicies(bool asXml = false)
    {
        _policyRepositoryWrapper.GetAllPolicies();

        return Ok();
    }
}