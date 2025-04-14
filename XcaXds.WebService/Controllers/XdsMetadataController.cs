using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Xca;
using XcaXds.Source;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class XdsMetadataController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly XdsConfig _xdsConfig;
    //private DocumentRegistry _documentRegistry;
    //private readonly RegistryWrapper _registryWrapper;

    public XdsMetadataController(ILogger<RegistryController> logger, XdsConfig xdsConfig, DocumentRegistry documentRegistry, RegistryWrapper registryWrapper)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        //_documentRegistry = documentRegistry;
        //_registryWrapper = registryWrapper;

        //_documentRegistry = _registryWrapper.GetDocumentRegistryContent() ?? new DocumentRegistry();
    }

    [Produces("application/json")]
    [HttpGet("about/config")]
    public async Task<IActionResult> GetXdsConfig()
    {
        return Ok(_xdsConfig);
    }

    //[Produces("application/json")]
    //[HttpGet("about/patients")]
    //public async Task<IActionResult> GetPatientsInRegistry()
    //{
    //    var patientIds = _documentRegistry.RegistryObjectList.OfType<ExtrinsicObjectType>()
    //        .Select(eo => eo.ExternalIdentifier.FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.PatientId).Value).ToList();
    //    return Ok(patientIds);
    //}
}