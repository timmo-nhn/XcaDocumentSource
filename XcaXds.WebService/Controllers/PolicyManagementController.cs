using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy.RequestMappers;
using XcaXds.WebService.Services.XdsRegistry;

namespace XcaXds.WebService.Controllers;

[RequiresApiKey]
[ApiController]
[Route("api/policy")]
public class PolicyManagementController : ControllerBase
{
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<PolicyManagementController> _logger;
    private readonly SamlPolicyRequestMapper _policyRequestMapperSamlService;
    public PolicyManagementController(PolicyRepositoryService policyRepositoryService, RegistryWrapper registryWrapper, ILogger<PolicyManagementController> logger, SamlPolicyRequestMapper policyRequestMapperSamlService)

    {
        _policyRequestMapperSamlService = policyRequestMapperSamlService;
        _policyRepositoryService = policyRepositoryService;
        _registryWrapper = registryWrapper;
        _logger = logger;
    }

    [Produces("application/json", "application/xml")]
    [HttpGet("get-all")]
    public IActionResult GetAllPolicies()
    {
        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();

        _logger.LogInformation($"Returned PolicySet with {policySet.Policies?.Count ?? 0} Policies");
        
        return Ok(policySet);
    }

    [Produces("application/json", "application/xml")]
    [HttpGet("get-single")]
    public IActionResult GetSinglePolicy(string id)
    {
        var policySet = _policyRepositoryService.GetSinglePolicy(id);
        
        return Ok(policySet);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPost("upload")]
    public IActionResult CreatePolicy([FromBody] AbacPolicy abacPolicy)
    {
        var response = _policyRepositoryService.AddPolicy(abacPolicy);

        var apiResponse = new RestfulApiResponse()
        {
            Success = response
        };

        if (apiResponse.Success)
        {
            apiResponse.SetMessage($"Created Policy with id {abacPolicy.Id}");
            return Ok(apiResponse);
        }

        if (_policyRepositoryService.GetSinglePolicy(abacPolicy.Id) != null)
        {
            apiResponse.AddError("Conflict", "Resource already exists");
            apiResponse.SetMessage($"Policy with id {abacPolicy.Id} already exists!");
            return Conflict(apiResponse);
        }

        return BadRequest(apiResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPut("update")]
    public IActionResult UpdatePolicy([FromBody] AbacPolicy abacPolicy, string? id)
    {
        var apiResponse = new RestfulApiResponse();

        var policyToUpdate = _policyRepositoryService.GetSinglePolicy(id ?? abacPolicy.Id);

        if (policyToUpdate == null)
        {
            apiResponse.SetMessage($"Policy with id {id ?? abacPolicy.Id} not found.");
            return NotFound(apiResponse);
        }

        var response = _policyRepositoryService.UpdatePolicy(abacPolicy, id);

        if (response)
        {
            apiResponse.Success = true;
            apiResponse.SetMessage($"Updated Policy with id {abacPolicy.Id}");
            return Ok(apiResponse);
        }

        return BadRequest(apiResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPatch("patch")]
    public IActionResult PatchPolicy([FromBody] AbacPolicy abacPolicy, string? newId, bool? append)
    {

        var apiResponse = new RestfulApiResponse();

        if (_policyRepositoryService.GetSinglePolicy(newId) != null)
        {
            apiResponse.AddError("Conflict", "New ID cannot be the same as an existing ID");
            apiResponse.Success = false;
            return Conflict(apiResponse);
        }

        var response = _policyRepositoryService.PartiallyUpdatePolicy(abacPolicy, newId, append ?? false);

        apiResponse.Success = response;

        if (apiResponse.Success)
        {
            apiResponse.SetMessage($"Created Policy with id {abacPolicy.Id}");
            return Ok(apiResponse);
        }

        return BadRequest(apiResponse);
    }


    [Produces("application/json")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeletePolicy(string id)
    {
        var response = _policyRepositoryService.DeletePolicy(id);
        var apiResponse = new RestfulApiResponse()
        {
            Success = response
        };

        if (apiResponse.Success)
        {
            apiResponse.SetMessage($"Succesfully deleted id {id}");
            return Ok(apiResponse);
        }

        apiResponse.SetMessage($"Policy {id} not found");
        return NotFound(apiResponse);
    }

    [Produces("application/json")]
    [HttpDelete("delete-all-policies")]
    public async Task<IActionResult> DeleteAllPolicies()
    {
        var result = _policyRepositoryService.DeleteAllPolicies();
        var apiResponse = new RestfulApiResponse()
        {
            Success = result
        };

        apiResponse.SetMessage($"Succesfully deleted all policies");
        return Ok(apiResponse);
    }
}