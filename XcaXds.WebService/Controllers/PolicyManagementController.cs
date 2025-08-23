using Abc.Xacml;
using Abc.Xacml.Policy;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api/policy")]
public class PolicyManagementController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly PolicyRepositoryService _policyRepositoryService;

    public PolicyManagementController
        (ILogger<XdsRegistryController> logger, 
        ApplicationConfig xdsConfig,
        RegistryWrapper registryWrapper, 
        RepositoryWrapper repositoryWrapper, 
        PolicyRepositoryService policyRepositoryService
        )
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _policyRepositoryService = policyRepositoryService;
    }

    [Produces("application/json","application/xml")]
    [HttpGet("getall")]
    public async Task<IActionResult> GetAllPolicies(bool asXml = false)
    {
        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();
        
        if (asXml)
        {
            var xacmlPolicySet = PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySet);
            
            var xmlPolicySet = XacmlSerializer.SerializeXacmlToXml(xacmlPolicySet);
            
            return Content(xmlPolicySet, Constants.MimeTypes.Xml);
            
        }

        return Ok(policySet);
    }

    [Produces("application/json")]
    [HttpPost("upload")]
    public async Task<IActionResult> CreatePolicy([FromBody]PolicyDto policyDto)
    {
        policyDto.SetDefaultValues();
        var response = _policyRepositoryService.AddPolicy(policyDto);

        var apiResponse = new RestfulApiResponse()
        {
            Success = response
        };

        if (apiResponse.Success)
        {
            apiResponse.SetMessage($"Created Policy with id {policyDto.Id}");
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
}