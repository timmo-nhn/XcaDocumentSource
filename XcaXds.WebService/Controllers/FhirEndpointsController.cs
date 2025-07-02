using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Controllers;

[Tags("FHIR Endpoints")]
[ApiController]
[Route("R4/fhir")]
public class FhirEndpointsController : Controller
{
    [HttpGet("DocumentReference")]
    public async Task<ActionResult> DocumentReference(
        [FromQuery(Name = "patient")] string patient,
        [FromQuery(Name = "creation")] string creation,
        [FromQuery(Name = "author.given")] string authorgiven,
        [FromQuery(Name = "author.family")] string authorfamily,
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "category")] string category,
        [FromQuery(Name = "type")] string type,
        [FromQuery(Name = "setting")] string setting,
        [FromQuery(Name = "period")] string period,
        [FromQuery(Name = "facility")] string facility,
        [FromQuery(Name = "event")] string @event,
        [FromQuery(Name = "security-label")] string securitylabel,
        [FromQuery(Name = "format")] string format
        )
    {
        var pretty = bool.Parse(Request.Headers["compact"].ToString() ?? "false");
        var resource = new Bundle() { Id = Guid.NewGuid().ToString() };

        var documentRequest = new MhdDocumentRequest()
        {
            Patient = patient,
            Creation = creation,
            Authorgiven = authorgiven,
            Authorfamily = authorfamily,
            Status = status,
            Category = category,
            Type = type,
            Setting = setting,
            Period = period,
            Facility = facility,
            Event = @event,
            Securitylabel = securitylabel,
            Format = format
        };

        var iti18 = XdsOnFhirService.GenerateIti18FromIti67(documentRequest);

        var fhirJsonSerializer = new FhirJsonSerializer(new SerializerSettings() { Pretty = pretty });
        var gobb = fhirJsonSerializer.SerializeToString(resource);

        return Ok(gobb);
    }
}