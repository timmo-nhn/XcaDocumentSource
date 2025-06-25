//using Microsoft.AspNetCore.Mvc;
//using XcaXds.Commons.Models.Custom.RegistryDtos;
//using XcaXds.Commons.Services;
//using XcaXds.OpenDipsRegistryRepository.Services;

//namespace XcaXds.WebService.Controllers
//{
//    [Route("api/[controller]")]
//    [Consumes("application/json")]
//    [Produces("application/json")]
//    [ApiController]
//    public class OpenDipsController : ControllerBase
//    {

//        private readonly FhirEndpointsDtoTransformerService _fhirEndpointsDtoTransformerService;

//        public OpenDipsController(FhirEndpointsDtoTransformerService fhirEndpointsDtoTransformerService)
//        {
//            _fhirEndpointsDtoTransformerService = fhirEndpointsDtoTransformerService;
//        }

//        [HttpGet("CreateRandomizedTestDataDocumentEntry")]
//        public async Task<IActionResult> CreateRandomizedTestDataDocumentEntry(string apiKey, string patientId = null)
//        {
//            var bundleDocumentList = await _fhirEndpointsDtoTransformerService.GetDocumentReference(patientId, apiKey);

//            var externalIdentifiers = _fhirEndpointsDtoTransformerService.GetResourceIdentifiersFromDocumentEntries(bundleDocumentList);

//            foreach (var resourceId in externalIdentifiers)
//            {
//                var resource = await _fhirEndpointsDtoTransformerService.GetResource(resourceId, apiKey);
//                _fhirEndpointsDtoTransformerService.AddResource(resource);
//            }


//            var registryObjects = new List<RegistryObjectDto>();

//            foreach (var documentReferenec in bundleDocumentList.Entry)
//            {
//                registryObjects.AddRange(FhirTransformerService.TransformFhirResourceToRegistryObjectDto(documentReferenec));
//            }


//            var documententry = new DocumentReferenceDto()
//            {

//            };


//            return Ok();
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            return Ok();
//        }
//    }
//}
