using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Controllers;

[RequiresApiKey]
[ApiController]
[Route("api/policy")]
public class PolicyManagementController : ControllerBase
{
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly PolicyDecisionPointService _policyDecisionPointService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<PolicyManagementController> _logger;
    private readonly PolicyRequestMapperSamlService _policyRequestMapperSamlService;
    public PolicyManagementController(PolicyRepositoryService policyRepositoryService, PolicyDecisionPointService policyDecisionPointService, RegistryWrapper registryWrapper, ILogger<PolicyManagementController> logger, PolicyRequestMapperSamlService policyRequestMapperSamlService)

    {
        _policyRequestMapperSamlService = policyRequestMapperSamlService;
        _policyRepositoryService = policyRepositoryService;
        _policyDecisionPointService = policyDecisionPointService;
        _registryWrapper = registryWrapper;
        _logger = logger;
    }

    [Produces("application/json", "application/xml")]
    [HttpGet("get-all")]
    public IActionResult GetAllPolicies(bool xml = false)
    {
        _logger.LogInformation($"Received request to get all policies. Returning in XML format: {xml}");

        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();

        _logger.LogInformation($"Returned PolicySet with {policySet.Policies?.Count ?? 0} Policies");

        if (xml)
        {
            var xacmlPolicySet = PolicyDtoTransformer.TransformPolicySetDtoToXacmlVersion20PolicySet(policySet);

            var xmlPolicySet = XacmlSerializer.SerializeXacmlToXml(xacmlPolicySet);

            if (string.IsNullOrWhiteSpace(xmlPolicySet)) return NotFound();

            return Content(xmlPolicySet, Constants.MimeTypes.Xml);
        }

        return Ok(policySet);
    }

    [Produces("application/json", "application/xml")]
    [HttpGet("get-single")]
    public IActionResult GetSinglePolicy(string id, bool xml = false)
    {
        var policySet = _policyRepositoryService.GetSinglePolicy(id);

        if (xml)
        {
            var xacmlPolicySet = PolicyDtoTransformer.TransformPolicyDtoToXacmlVersion20Policy(policySet);

            var xmlPolicySet = XacmlSerializer.SerializeXacmlToXml(xacmlPolicySet);
            if (string.IsNullOrWhiteSpace(xmlPolicySet)) return NotFound();

            return Content(xmlPolicySet, Constants.MimeTypes.Xml);
        }

        return Ok(policySet);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPost("upload")]
    public IActionResult CreatePolicy([FromBody] PolicyDto policyDto)
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
    public IActionResult UpdatePolicy([FromBody] PolicyDto policyDto, string? id)
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
            apiResponse.SetMessage($"Updated Policy with id {policyDto.Id}");
            return Ok(apiResponse);
        }

        return BadRequest(apiResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPatch("patch")]
    public IActionResult PatchPolicy([FromBody] PolicyDto policyDto, string? newId, bool? append)
    {

        var apiResponse = new RestfulApiResponse();

        if (_policyRepositoryService.GetSinglePolicy(newId) != null)
        {
            apiResponse.AddError("Conflict", "New ID cannot be the same as an existing ID");
            apiResponse.Success = false;
            return Conflict(apiResponse);
        }

        var response = _policyRepositoryService.PartiallyUpdatePolicy(policyDto, newId, append ?? false);

        apiResponse.Success = response;

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

    [Consumes("application/soap+xml")]
    [Produces("application/json")]
    [HttpPost("evaluate-soap-request")]
    public async Task<IActionResult> GetXacmlRequest([FromBody] SoapEnvelope soapEnvelope)
    {
        var response = new RestfulApiResponse();

        var xacmlRequest = _policyRequestMapperSamlService.GetXacmlRequest(soapEnvelope);

        var evaluationResponse = _policyDecisionPointService.EvaluateXacmlRequest(xacmlRequest);

        var result = evaluationResponse.Results.Select(res => res.Decision).FirstOrDefault().ToString();

        response.SetMessage($"Result from policy evaluation: {result}");

        return Ok(response);
    }
}