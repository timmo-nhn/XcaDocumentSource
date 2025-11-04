using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
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
using XcaXds.WebService.Models.Custom;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("R4/fhir")]
[Tags("FHIR Endpoints")]
[UsePolicyEnforcementPoint]
public class FhirEndpointsController : Controller
{
    private readonly ILogger<FhirEndpointsController> _logger;

    private readonly XdsRegistryService _xdsRegistryService;
    private readonly XdsRepositoryService _xdsRepositoryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly XdsOnFhirService _xdsOnFhirService;
    private readonly ApplicationConfig _appConfig;

    public FhirEndpointsController(
        ILogger<FhirEndpointsController> logger,
        XdsRegistryService xdsRegistryService, 
        XdsRepositoryService xdsRepositoryService,
        XdsOnFhirService xdsOnFhirService, 
        RegistryWrapper registryWrapper, 
        RepositoryWrapper repositoryWrapper,
        ApplicationConfig applicationConfig
        )
    {
        _xdsRegistryService = xdsRegistryService;
        _xdsRepositoryService = xdsRepositoryService;
        _xdsOnFhirService = xdsOnFhirService;
        _logger = logger;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _appConfig = applicationConfig;
    }

    [Consumes("application/fhir+json")]
    [Produces("application/fhir+json")]
    [HttpGet("DocumentReference")]
    [HttpPost("DocumentReference/_search")]
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

        var response = _xdsRegistryService.RegistryStoredQueryAsync(soapEnvelope);

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

    [Consumes("application/fhir+json", "application/fhir+xml")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpPost("Bundle")]
    public async Task<IActionResult> DocumentReference([FromBody] JsonElement json)
    {
        var fhirParser = new FhirJsonParser();
        var resource = fhirParser.Parse<Resource>(json.GetRawText());

        if (resource is not Bundle fhirBundle)
        {
            return BadRequest(OperationOutcome
            .ForMessage($"Request body does not contain a well formatted FHIR bundle",
            OperationOutcome.IssueType.Invalid,
            OperationOutcome.IssueSeverity.Fatal));
        }

        var patient = fhirBundle.Entry
            .Where(e => e.Resource is Patient)
            .Select(e => (Patient)e.Resource)
            .FirstOrDefault();

        var documentReferences = fhirBundle.Entry
            .Where(e => e.Resource is DocumentReference)
            .Select(e => (DocumentReference)e.Resource)
            .ToList();

        var fhirBinaries = fhirBundle.Entry
            .Where(e => e.Resource is Binary)
            .Select(e => (Binary)e.Resource)
            .ToList();

        var submissionSetList = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<List>()
            .FirstOrDefault();

        if (submissionSetList == null) return BadRequest(OperationOutcome.ForMessage($"List element is missing or malformed", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        if (documentReferences.Count == 0) return BadRequest(OperationOutcome.ForMessage($"DocumentReference element is missing or malformed", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        if (fhirBinaries.Count == 0) return BadRequest(OperationOutcome.ForMessage($"Binary element is missing or malformed", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        if (patient == null) return BadRequest(OperationOutcome.ForMessage($"Patient not found in DocumentReference", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        var identifier = patient.Identifier.First();

        var patientIdCodeSystem = identifier.System?.NoUrn();
        var patientIdentifier = identifier.Value;

        var sourceIdIdentifier = submissionSetList.GetExtension("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-sourceId").Value;
        var sourceId = sourceIdIdentifier.First().Value.ToString();

        if (string.IsNullOrEmpty(sourceId))
        {
            return BadRequestOperationOutcome.Create(OperationOutcome.ForMessage("Missing SourceID", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));
        }

        var provideAndRegisterResult = BundleProcessorService.CreateSoapObjectFromComprehensiveBundle(documentReferences, submissionSetList, fhirBinaries, identifier, patientIdCodeSystem?.NoUrn());

        var provideAndRegisterRequest = provideAndRegisterResult.Value?.ProvideAndRegisterDocumentSetRequest;

        if (provideAndRegisterResult.Success == false && provideAndRegisterResult.OperationOutcome != null && provideAndRegisterRequest != null)
        {
            return BadRequestOperationOutcome.Create(provideAndRegisterResult.OperationOutcome);
        }

        // FIXME Handle errors
        var submittedDocumentsTooLarge = _xdsRepositoryService.CheckIfDocumentsAreTooLarge(provideAndRegisterRequest);

        // FIXME Handle errors
        var iti42Message = _xdsRegistryService.CopyIti41ToIti42Message(provideAndRegisterRequest);

        // FIXME Handle errors
        var repositoryDocumentExists = _xdsRepositoryService.CheckIfDocumentExistsInRepository(provideAndRegisterRequest);

        // FIXME Handle errors
        var registerDocumentSetResponse = _xdsRegistryService.AppendToRegistryAsync(iti42Message.Value);

        // FIXME Handle errors
        var documentUploadResponse = _xdsRepositoryService.UploadContentToRepository(provideAndRegisterRequest);

        return Ok(OperationOutcome.ForMessage($"Document with id {fhirBundle.Id} has been uploaded successfully", OperationOutcome.IssueType.Success, OperationOutcome.IssueSeverity.Success));
    }
}
