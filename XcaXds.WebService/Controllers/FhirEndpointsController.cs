using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;

[Tags("FHIR Endpoints")]
[ApiController]
[UsePolicyEnforcementPoint]
public class FhirEndpointsController : Controller
{
    private readonly ILogger<FhirEndpointsController> _logger;

    private readonly XdsRegistryService _xdsRegistryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly XdsOnFhirService _xdsOnFhirService;

    public FhirEndpointsController(ILogger<FhirEndpointsController> logger, XdsRegistryService xdsRegistryService, XdsOnFhirService xdsOnFhirService, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper)
    {
        _xdsRegistryService = xdsRegistryService;
        _xdsOnFhirService = xdsOnFhirService;
        _logger = logger;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
    }

    [Consumes("application/fhir+json")]
    [Produces("application/fhir+json")]
    [HttpGet("R4/fhir/DocumentReference")]
    [HttpPost("R4/fhir/DocumentReference/_search")]
    public async Task<ActionResult> DocumentReference(
        [FromQuery(Name = "patient")] string? patient,
        [FromQuery(Name = "creation")] string? creation,
        [FromQuery(Name = "author.given")] string? authorGiven,
        [FromQuery(Name = "author.family")] string? authorFamily,
        [FromQuery(Name = "status")] string? status,
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
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"Received request for action: ITI-67 from {Request.HttpContext.Connection.RemoteIpAddress}");

        var prettyprint = string.IsNullOrWhiteSpace(Request.Headers["compact"].ToString())
            ? "true"
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

        var operationOutcome = new OperationOutcome();

        var fhirJsonSerializer = new FhirJsonSerializer(new SerializerSettings() { Pretty = pretty });

        if (string.IsNullOrWhiteSpace(status))
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = "The 'status' field is required."
            });

            _logger.LogInformation($"Missing required field 'status'");
        }

        if (string.IsNullOrWhiteSpace(patient))
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = "The 'patient' field is required."
            });

            _logger.LogInformation($"Missing required field 'patient'");
        }

        if (operationOutcome.Issue.Count != 0)
        {
            requestTimer.Stop();
            _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
            return BadRequest(fhirJsonSerializer.SerializeToString(operationOutcome));
        }

        var adhocQueryRequest = new AdhocQueryRequest();
        var adhocQuery = _xdsOnFhirService.ConvertIti67ToIti18AdhocQuery(documentRequest).AdhocQuery;

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

        var bundle = _xdsOnFhirService.TransformRegistryObjectsToFhirBundle(response.Value?.Body?.AdhocQueryResponse?.RegistryObjectList);

        requestTimer.Stop();

        _logger.LogInformation($"Number of Bundle.Entries {bundle?.Entry?.Count ?? 0}");

        if (bundle != null)
        {
            var jsonOutput = fhirJsonSerializer.SerializeToString(bundle);

            _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
            return Content(jsonOutput);
        }

        _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
        return BadRequest(operationOutcome);
    }


    [HttpGet("mhd/document")]
    public async Task<IActionResult> Document(
        [FromQuery] string homeCommunityId,
        [FromQuery] string repositoryUniqueId,
        [FromQuery] string documentUniqueId
        )
    {
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"Received request for action: ITI-68 from {Request.HttpContext.Connection.RemoteIpAddress}");

        var registryObjectForDocument = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.Id == documentUniqueId);

        if (registryObjectForDocument?.AvailabilityStatus == Constants.Xds.StatusValues.Deprecated)
            return StatusCode(StatusCodes.Status410Gone);

        var document = _repositoryWrapper.GetDocumentFromRepository(homeCommunityId, repositoryUniqueId, documentUniqueId);

        requestTimer.Stop();

        var mimetype = StringExtensions.GetMimetypeFromMagicNumber(document);

        if (document == null)
        {
            _logger.LogInformation($"No document with id {documentUniqueId} found");
            return NotFound();
        }

        _logger.LogInformation($"Returned document. Mimetype {mimetype ?? registryObjectForDocument?.MimeType ?? "unknown"}");
        _logger.LogInformation($"Completed action: ITI-68 in {requestTimer.ElapsedMilliseconds} ms with 0 errors");


        var fileResponse = File(document, mimetype ?? registryObjectForDocument.MimeType);
        fileResponse.FileDownloadName = $"{documentUniqueId}.{mimetype?.Split('/')[1] ?? registryObjectForDocument?.MimeType?.Split('/')[1]}";

        return fileResponse;
    }
}
