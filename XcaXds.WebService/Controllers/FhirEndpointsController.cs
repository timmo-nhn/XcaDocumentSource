using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[Tags("FHIR Endpoints")]
[ApiController]
[Route("R4/fhir")]
public class FhirEndpointsController : Controller
{
    private readonly ILogger<FhirEndpointsController> _logger;

    private readonly XdsRegistryService _xdsRegistryService;

    public FhirEndpointsController(ILogger<FhirEndpointsController> logger, XdsRegistryService xdsRegistryService)
    {
        _xdsRegistryService = xdsRegistryService;
        _logger = logger;
    }

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

        var documentRequest = new MhdDocumentRequest()
        {
            Patient = HttpUtility.UrlDecode(patient),
            Creation = HttpUtility.UrlDecode(creation),
            AuthorGiven = HttpUtility.UrlDecode(authorGiven),
            AuthorFamily = HttpUtility.UrlDecode(authorFamily),
            Status = HttpUtility.UrlDecode(status),
            Category = HttpUtility.UrlDecode(category),
            Type = HttpUtility.UrlDecode(typeCode),
            Setting = HttpUtility.UrlDecode(setting),
            Period = HttpUtility.UrlDecode(period),
            Facility = HttpUtility.UrlDecode(facility),
            Event = HttpUtility.UrlDecode(eventCode),
            Securitylabel = HttpUtility.UrlDecode(securityLabel),
            Format = HttpUtility.UrlDecode(format)
        };

        var adhocQueryRequest = new AdhocQueryRequest();
        var adhocQuery = XdsOnFhirService.ConvertIti67ToIti18AdhocQuery(documentRequest).AdhocQuery;

        adhocQueryRequest.AdhocQuery = adhocQuery;
        adhocQueryRequest.AdhocQuery.Id = Constants.Xds.StoredQueries.FindDocuments;

        adhocQueryRequest.ResponseOption = new()
        {
            ReturnType = ResponseOptionTypeReturnType.LeafClass
        };
        
        var soapEnvelope = new SoapEnvelope()
        {
            Header = new(),
            Body = new() { AdhocQueryRequest = adhocQueryRequest }
        };

        var response = await _xdsRegistryService.RegistryStoredQueryAsync(soapEnvelope);
        
        var bundle = XdsOnFhirService.TransformRegistryObjectsToFhirBundle(response.Value.Body.AdhocQueryResponse.RegistryObjectList);



        var fhirJsonSerializer = new FhirJsonSerializer(new SerializerSettings() { Pretty = pretty });
        var gobb = fhirJsonSerializer.SerializeToString(bundle);

        return Ok(gobb);
    }
}