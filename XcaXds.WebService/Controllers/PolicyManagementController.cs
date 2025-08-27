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
    public IActionResult GetAllPolicies(bool xml = false)
    {
        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();
        
        if (xml)
        {
            var xacmlPolicySet = PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySet);
            
            var xmlPolicySet = XacmlSerializer.SerializeXacmlToXml(xacmlPolicySet);
            
            return Content(xmlPolicySet, Constants.MimeTypes.Xml);
            
        }

        return Ok(policySet);
    }

    [Produces("application/json","application/xml")]
    [HttpGet("getsingle")]
    public IActionResult GetSinglePolicy(string id, bool xml = false)
    {
        var policySet = _policyRepositoryService.GetSinglePolicy(id);
        
        if (xml)
        {
            var xacmlPolicySet = PolicyDtoTransformerService.TransformPolicyDtoToXacmlVersion20Policy(policySet);
            
            var xmlPolicySet = XacmlSerializer.SerializeXacmlToXml(xacmlPolicySet);
            
            return Content(xmlPolicySet, Constants.MimeTypes.Xml);
        }

        return Ok(policySet);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPost("upload")]
    public IActionResult CreatePolicy([FromBody]PolicyDto policyDto)
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

        if (_policyRepositoryService.GetSinglePolicy(policyDto.Id) != null)
        {
            apiResponse.AddError("Conflict", "Resource already exists");
            apiResponse.SetMessage($"Policy with id {policyDto.Id} already exists!");
            return Conflict(apiResponse);
        }

        return BadRequest(apiResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPut("update")]
    public IActionResult UpdatePolicy([FromBody]PolicyDto policyDto, string? id)
    {
        var apiResponse = new RestfulApiResponse();

        var policyToUpdate = _policyRepositoryService.GetSinglePolicy(id ?? policyDto.Id);

        if (policyToUpdate == null)
        {
            apiResponse.SetMessage($"Policy with id {id ?? policyDto.Id} not found.");
            return NotFound(apiResponse);
        }

        var response = _policyRepositoryService.UpdatePolicy(policyDto, id);

        if (response)
        {
            apiResponse.Success = true;
            apiResponse.SetMessage($"Created Policy with id {policyDto.Id}");
            return Ok(apiResponse);
        }

        return BadRequest(apiResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPatch("patch")]
    public IActionResult PatchPolicy([FromBody]PolicyDto policyDto, string? id)
    {
        policyDto.SetDefaultValues();
        var response = _policyRepositoryService.PartiallyUpdatePolicy(policyDto, id);

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