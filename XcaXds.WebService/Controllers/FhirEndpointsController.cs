using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace XcaXds.WebService.Controllers;

[Tags("FHIR Endpoints")]
[ApiController]
[Route("R4/fhir")]
public class FhirEndpointsController : Controller
{

    [HttpGet("DocumentReference/_search")]
    public async Task<ActionResult> Index()
    {
        var resource = new Bundle() { Id = Guid.NewGuid().ToString() };

        var fhirJsonSerializer = new FhirJsonSerializer(new SerializerSettings() { Pretty = true });

        var gobb = fhirJsonSerializer.SerializeToString(resource);

        return Ok(gobb);
    }
}