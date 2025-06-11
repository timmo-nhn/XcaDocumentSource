using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Source.Services.Custom;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XcaXds.WebService.Controllers
{
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiController]
    public class OpenDipsController : ControllerBase
    {
        private readonly OpenDipsClient _openDipsClient;
        private readonly HttpClient _httpClient;

        public OpenDipsController(OpenDipsClient openDipsClient, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _openDipsClient = openDipsClient;
        }

        [HttpGet("CreateRandomizedTestDataDocumentEntry")]
        public async Task<IActionResult> CreateRandomizedTestDataDocumentEntry(string apiKey, string patientId = null)
        {
            // Documentlist

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["Patient.Identifier"] = patientId;

            var responseContent = _openDipsClient.CallApiAsync("https://api.dips.no/fhir/DocumentReference?" + queryParams.ToString());

            var fhirSerializer = new FhirJsonSerializer();


            var documententry = new DocumentReferenceDto()
            {

            };


            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok();
        }
    }
}
