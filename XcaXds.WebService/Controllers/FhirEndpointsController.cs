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
        [FromQuery(Name = "creation")] string? creation,
        [FromQuery(Name = "author.given")] string? authorGiven,
        [FromQuery(Name = "author.family")] string? authorFamily,
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "category")] string? category,
        [FromQuery(Name = "type")] string? typeCode,
        [FromQuery(Name = "setting")] string? setting,
        [FromQuery(Name = "period")] string? period,
        [FromQuery(Name = "facility")] string? facility,
        [FromQuery(Name = "event")] string? eventCode,
        [FromQuery(Name = "security-label")] string? securityLabel,
        [FromQuery(Name = "format")] string? format
        )
    {
        var prettyprint = string.IsNullOrWhiteSpace(Request.Headers["compact"].ToString()) 
            ? "false"
            : Request.Headers["compact"].ToString();

        var pretty = bool.Parse(prettyprint);
        var resource = new Bundle() { Id = Guid.NewGuid().ToString() };

        var documentRequest = new MhdDocumentRequest()
        {
            Patient = patient,
            Creation = creation,
            AuthorGiven = authorGiven,
            AuthorFamily = authorFamily,
            Status = status,
            Category = category,
            Type = typeCode,
            Setting = setting,
            Period = period,
            Facility = facility,
            Event = eventCode,
            Securitylabel = securityLabel,
            Format = format
        };

        var iti18 = XdsOnFhirService.GenerateIti18FromIti67(documentRequest);

        var fhirJsonSerializer = new FhirJsonSerializer(new SerializerSettings() { Pretty = pretty });
        var gobb = fhirJsonSerializer.SerializeToString(resource);

        return Ok(gobb);
    }
}