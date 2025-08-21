using Abc.Xacml;
using Abc.Xacml.Policy;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
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
    private readonly PolicyRepositoryService _policyRepositoryService;

    public PolicyManagementController(ILogger<XdsRegistryController> logger, 
        ApplicationConfig xdsConfig,
        RegistryWrapper registryWrapper, 
        RepositoryWrapper repositoryWrapper, 
        PolicyRepositoryService policyRepositoryService)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _policyRepositoryService = policyRepositoryService;
    }

    [Produces("application/json","application/xml")]
    [HttpGet("policy/getall")]
    public async Task<IActionResult> GetAllPolicies(bool asXml = false)
    {
        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();
        
        if (asXml)
        {
            var serializer = new Xacml20ProtocolSerializer();

            var sb = new StringBuilder();

            using (var writer = XmlWriter.Create(sb, Constants.XmlDefaultOptions.DefaultXmlWriterSettings))
            {
                var serialize = new Xacml20ProtocolSerializer();
                serialize.WritePolicySet(writer, PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySet));
                var gogg = sb.ToString();
                return Content(gogg, Constants.MimeTypes.Xml);
            }
        }

        return Ok(policySet);
    }
}