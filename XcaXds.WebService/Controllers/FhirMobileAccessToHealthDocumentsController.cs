using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Web;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.DomainResults;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using XcaXds.WebService.Models.Custom;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.AtnaAuditLogging;
using XcaXds.WebService.Services.Fhir;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("R4/fhir")]
[Tags("FHIR Endpoints")]
[UsePolicyEnforcementPoint]
[ExportsStatistics]
public class FhirMobileAccessToHealthDocumentsController : Controller
{
    private readonly ILogger<FhirMobileAccessToHealthDocumentsController> _logger;
    private readonly MonitoringStatusService _monitoringStatusService;
    private readonly DocumentListFiltererService _documentListFiltererService;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RegistryMetadataTransformerService _registryMetadataTransformerService;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly FhirService _fhirService;
    private readonly AtnaLogGeneratorService _atnaLoggingService;
    private readonly AtnaLogEnricherService _atnaLogEnricherService;
    private readonly FhirResourceValidatorService _fhirValidator;
    private readonly XdsOnFhirTransformerService _xdsOnFhirTransformerService;
    private readonly XdsRegistryService _xdsRegistryService;
    private readonly IVariantFeatureManager _featureManager;


    public FhirMobileAccessToHealthDocumentsController(
        ILogger<FhirMobileAccessToHealthDocumentsController> logger,
        MonitoringStatusService monitoringStatusService,
        XdsRegistryService xdsRegistryService,
        RegistryMetadataTransformerService registryMetadataTransformerService,
        XdsRepositoryService xdsRepositoryService,
        RestfulRegistryRepositoryService restfulRegistryService,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper,
        ApplicationConfig applicationConfig,
        AtnaLogGeneratorService atnaLoggingService,
        AtnaLogEnricherService atnaLogEnricherService,
        FhirService fhirService,
        FhirResourceValidatorService fhirValidator,
        XdsOnFhirTransformerService xdsOnFhirTransformerService,
        IVariantFeatureManager featureManager,
        DocumentListFiltererService documentListFiltererService)
    {
        _logger = logger;
        _monitoringStatusService = monitoringStatusService;
        _registryMetadataTransformerService = registryMetadataTransformerService;
        _fhirService = fhirService;
        _fhirValidator = fhirValidator;
        _featureManager = featureManager;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _xdsRegistryService = xdsRegistryService;
        _atnaLoggingService = atnaLoggingService;
        _atnaLogEnricherService = atnaLogEnricherService;
        _restfulRegistryService = restfulRegistryService;
        _documentListFiltererService = documentListFiltererService;
        _xdsOnFhirTransformerService = xdsOnFhirTransformerService;
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
        if (!await _featureManager.IsEnabledAsync("Fhir_DocumentReference")) return NotFound();

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

        var fhirJsonSerializer = new FhirJsonSerializer();

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

        if (operationOutcome.Issue.Count > 0)
        {
            requestTimer.Stop();
            _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
            return BadRequest(fhirJsonSerializer.SerializeToString(operationOutcome));
        }

        var adhocQueryRequest = new AdhocQueryRequest()
        {
            AdhocQuery = _xdsOnFhirTransformerService.ConvertIti67ToIti18AdhocQuery(documentRequest).AdhocQuery,
            Id = Constants.Xds.StoredQueries.FindDocuments,
            ResponseOption = new()
            {
                ReturnType = ResponseOptionTypeReturnType.LeafClass
            }
        };

        var soapEnvelope = new SoapEnvelope()
        {
            Header = new(),
            Body = new() { AdhocQueryRequest = adhocQueryRequest }
        };

        var abacRequest = HttpContext.Items.TryGetValue("accessRequest", out var accessRequest) ? accessRequest as AbacRequest : null;

        var registryQueryResponse = _xdsRegistryService.RegistryStoredQuery(soapEnvelope);
        var filteredDocumentList = _documentListFiltererService.FilterAdhocQueryResponseBasedOnBusinessLogic(soapEnvelope, registryQueryResponse.Value, abacRequest, out var businessLogicResults);
        HttpContext.Items.Add("businessLogicResult", businessLogicResults);

        var bundle = _xdsOnFhirTransformerService.TransformRegistryObjectsToFhirBundle(registryQueryResponse.Value?.Body.AdhocQueryResponse?.RegistryObjectList, _registryWrapper.GetDocumentRegistryContentAsDtos());

        requestTimer.Stop();

        _logger.LogInformation($"Number of Bundle.Entries {bundle?.Entry?.Count ?? 0}");

        if (bundle != null)
        {
            var jsonOutput = fhirJsonSerializer.SerializeToString(bundle);

            _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
            return Content(jsonOutput);
        }

        _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
        return BadRequestOperationOutcome.Create(operationOutcome);
    }

    [RequiresApiKey]
    [HttpGet("mhd/document")]
    public async Task<IActionResult> Document(
        [FromQuery] string homeCommunityId,
        [FromQuery] string repositoryUniqueId,
        [FromQuery] string documentUniqueId
    )
    {
        if (!await _featureManager.IsEnabledAsync("Fhir_ReadDocument")) return NotFound();

        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"{HttpContext.TraceIdentifier} - Received request for action: ITI-68 from {Request.HttpContext.Connection.RemoteIpAddress}");

        var registryObjectForDocument = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.Id == documentUniqueId);

        if (registryObjectForDocument?.AvailabilityStatus == Constants.Xds.StatusValues.Deprecated)
            return StatusCode(StatusCodes.Status410Gone);

        var document = _repositoryWrapper.GetDocumentFromRepository(homeCommunityId, repositoryUniqueId, documentUniqueId, out _);

        requestTimer.Stop();

        var mimetype = MimeTypeExtensions.TryGetMimeTypeFromDocumentBytes(document, out var mime) ? mime : null;

        if (document == null)
        {
            _logger.LogInformation($"No document with id {documentUniqueId} found");
            return NotFound();
        }

        _logger.LogInformation($"Returned document. MimeType {mimetype ?? registryObjectForDocument?.MimeType ?? "unknown"}");
        _logger.LogInformation($"Completed action: ITI-68 in {requestTimer.ElapsedMilliseconds} ms with 0 errors");


        var fileResponse = File(document, mimetype ?? registryObjectForDocument?.MimeType ?? "Unknown");
        fileResponse.FileDownloadName = $"{documentUniqueId}.{mimetype?.Split('/')[1] ?? registryObjectForDocument?.MimeType?.Split('/')[1]}";

        return fileResponse;
    }

    [RequiresApiKey]
    [HttpGet("DocumentReference/{id}")]
    public async Task<IActionResult> GetDocumentReference(string id)
    {
        if (!await _featureManager.IsEnabledAsync("Fhir_GetDocumentReference")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var registryItems = _registryWrapper.GetRegistryItemAndRelated(id);
        var registryObjects = RegistryMetadataTransformerService.TransformRegistryObjectDtosToRegistryObjectsStateless(registryItems).ToArray();

        var documentReference = _xdsOnFhirTransformerService.GetFhirDocumentReferencesFromRegistryObjects(registryObjects).FirstOrDefault();
        var options = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();
        var jsonResult = JsonSerializer.Serialize(documentReference, options);

        requestTimer.Stop();

        _logger.LogInformation($"Completed action: GetDocumentReference in {requestTimer.ElapsedMilliseconds}ms");

        return Content(jsonResult, Constants.MimeTypes.FhirJson);
    }

    [RequiresApiKey]
    [ExportsAtnaAuditLog]
    [Consumes("application/fhir+json", "application/fhir+xml")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpDelete("DocumentReference/{id}")]
    public async Task<IActionResult> DeleteDocument(string id)
    {
        if (!await _featureManager.IsEnabledAsync("Fhir_DeleteDocuments")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        _logger.LogInformation($"{HttpContext.TraceIdentifier} - Received request to delete document with id {id} from {Request.HttpContext.Connection.RemoteIpAddress}");
        var operationOutcome = new OperationOutcome();

        var deleteResponse = _restfulRegistryService.DeleteDocumentAndMetadata(id, out var deletedEntry);

        if (deletedEntry != null)
        {
            HttpContext.Items.Add("deletedRegistryObjects", new List<DocumentEntryDto> { deletedEntry });
        }

        if (deleteResponse.Success)
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Information,
                Code = OperationOutcome.IssueType.Success,
                Diagnostics = $"Document and associated metadata was successfully deleted from the Document Registry and Repository"
            });
        }
        else
        {
            foreach (var ooc in deleteResponse.Errors ?? [])
            {
                operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Value,
                    Diagnostics = $"{ooc.Code}: {ooc.Message}",
                    Location = [id]
                });
            }
        }

        requestTimer.Stop();

        _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.DocumentReferenceDelete, requestTimer.ElapsedMilliseconds);

        _logger.LogInformation($"Completed action: Delete DocumentReference in {requestTimer.ElapsedMilliseconds}ms with {operationOutcome.Issue.Count} issues");

        var anyErrors = operationOutcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueSeverity.Fatal);

        if (anyErrors)
        {
            return BadRequestOperationOutcome.Create(operationOutcome);
        }

        return OkOperationOutcome.Create(operationOutcome);
    }

    [RequiresApiKey]
    [ExportsAtnaAuditLog]
    //[RequestSizeLimit(Program.OneHundredMb)] // Can be used to override options.Limits.MaxRequestBodySize in Program.cs
    [Consumes("application/fhir+json", "application/fhir+xml")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpPost("Bundle")]
    public async Task<IActionResult> ProvideBundle([FromBody] JsonElement json)
    {
        if (!await _featureManager.IsEnabledAsync("Fhir_ProvideBundle")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Received request for action: ITI-65 ProvideBundle from {Request.HttpContext.Connection.RemoteIpAddress}");

        var fhirJsonDeserializer = new FhirJsonDeserializer();

        var fhirParser = new FhirJsonDeserializer();

        var rawJsonBundle = json.GetRawText();

        _logger.LogDebug($"{HttpContext.TraceIdentifier} - FHIR-Bundle:\n" + rawJsonBundle);

        var resource = fhirParser.DeserializeResource(rawJsonBundle);

        if (resource is not Bundle fhirBundle)
        {
            requestTimer.Stop();

            _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.Iti65Action, requestTimer.ElapsedMilliseconds);

            return BadRequestOperationOutcome.Create(OperationOutcome.ForMessage($"Request body does not contain a well formatted FHIR bundle",
                OperationOutcome.IssueType.Invalid,
                OperationOutcome.IssueSeverity.Fatal));
        }

        // Validate bundle first
        var validationResult = _fhirValidator.ValidateFhirResource(fhirBundle);
        var anyValidationErrors = validationResult.IssuesOfSeverity(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueSeverity.Fatal);

        bool effectiveValidate = false;

        // Add ProvideBundle validation aswell to avoid the user thinking that the only errors are the validation results
        // Preventing them from fighting "waves" of errors :P 
        if (anyValidationErrors)
        {
            effectiveValidate = true;
        }

        // Then provide
        var provideBundleResult = await _fhirService.ProvideBundle(fhirBundle, Request.HttpContext.TraceIdentifier, effectiveValidate);
        provideBundleResult.Outcome.Issue.AddRange(validationResult.Issue);

        // ATNA Audit Log generation

        HttpContext.Items.Add("uploadedEntries", provideBundleResult.ProvideAndRegisterRequest?.SubmitObjectsRequest?.RegistryObjectList);
        HttpContext.Items.Add("uploadedEntriesRegistryResponse", provideBundleResult.RegistryResponse);

        var anyProvideErrors = provideBundleResult.Outcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueSeverity.Fatal);

        if (anyProvideErrors)
        {
            requestTimer.Stop();

            _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.Iti65Action, requestTimer.ElapsedMilliseconds);

            return BadRequestOperationOutcome.Create(provideBundleResult.Outcome);
        }

        var transactionBundle = CreateFhirTransactionResponseBundle(fhirBundle);

        if (provideBundleResult.Outcome?.Issue?.Count > 0)
        {
            transactionBundle.Entry.Add(new() { Resource = provideBundleResult.Outcome });
        }

        var options = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();
        var jsonResult = JsonSerializer.Serialize(transactionBundle, options);
        requestTimer.Stop();

        _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.Iti65Action, requestTimer.ElapsedMilliseconds);

        _logger.LogInformation($"Completed action: ITI-65 ProvideBundle in {requestTimer.ElapsedMilliseconds}ms with {provideBundleResult.Outcome?.Issue?.Count ?? 0} issues");

        return Content(jsonResult, Constants.MimeTypes.FhirJson);
    }

    [RequiresApiKey]
    [ExportsAtnaAuditLog]
    [Consumes("application/fhir+json", "application/fhir+xml")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpPost("{resource}/$validate")]
    public async Task<IActionResult> ValidateResource([FromRoute] string? resource, [FromBody] JsonElement json)
    {
        var requestTimer = Stopwatch.StartNew();

        if (!await _featureManager.IsEnabledAsync("Fhir_ValidateResource")) return NotFound();

        var operationOutcome = new OperationOutcome();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Received $validate request for ResourceType {resource} from {Request.HttpContext.Connection.RemoteIpAddress}");

        var fhirJsonDeserializer = new FhirJsonDeserializer();

        var fhirParser = new FhirJsonDeserializer();
        var fhirResource = fhirParser.DeserializeResource(json.GetRawText());

        if (fhirResource is not Bundle fhirBundle)
        {
            requestTimer.Stop();

            _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.Iti65Action, requestTimer.ElapsedMilliseconds);

            return BadRequestOperationOutcome.Create(OperationOutcome.ForMessage($"Endpoint only supports validating FHIR bundles for now",
                OperationOutcome.IssueType.Invalid,
                OperationOutcome.IssueSeverity.Fatal));
        }

        // Validate bundle
        var provideBundleResult = await _fhirService.ProvideBundle(fhirBundle, Request.HttpContext.TraceIdentifier, validateOnly: true);
        var validationResult = _fhirValidator.ValidateFhirResource(fhirBundle);

        operationOutcome.Issue.AddRange(provideBundleResult.Outcome.Issue);
        operationOutcome.Issue.AddRange(validationResult.Issue);

        var fhirSerializer = new FhirJsonSerializer();

        var anyErrors = operationOutcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueSeverity.Fatal);

        if (!anyErrors)
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Information,
                Code = OperationOutcome.IssueType.Success,
                Diagnostics = $"Bundle validated with 0 errors or warnings"
            });
        }

        requestTimer.Stop();

        _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.Iti65ValidateAction, requestTimer.ElapsedMilliseconds);

        _logger.LogInformation($"Completed action: ITI-65 ValidateBundle in {requestTimer.ElapsedMilliseconds}ms with {provideBundleResult.Outcome?.Issue?.Count ?? 0} issues{((provideBundleResult.Outcome?.Issue?.Count ?? 0) == 0 ? ". Bundle is good to go!" : "")}");

        return new CustomContentResult(
            fhirSerializer.SerializeToString(operationOutcome),
            anyErrors ? StatusCodes.Status400BadRequest : StatusCodes.Status200OK,
            Constants.MimeTypes.FhirJson);
    }

    private Bundle CreateFhirTransactionResponseBundle(Bundle fhirBundle)
    {
        // --- MHD ProvideDocumentBundleResponse (Bundle type = transaction-response) ---
        var selfUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

        var responseBundle = new Bundle
        {
            Id = "bundle-response-" + Guid.NewGuid().ToString(),
            Type = Bundle.BundleType.TransactionResponse,
            Meta = new Meta
            {
                Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.ProvideDocumentBundleResponse"]
            },
            Link = new List<Bundle.LinkComponent>
            {
                new Bundle.LinkComponent
                {
                    Relation = "self",
                    Url = selfUrl
                }
            }
        };

        // One entry in the response for each entry in the request, in the same order
        var now = DateTimeOffset.UtcNow;

        var documentReferences = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<DocumentReference>()
            .ToList();

        foreach (var entry in documentReferences)
        {
            var resourceId = entry?.Id;

            if (string.IsNullOrEmpty(resourceId))
            {
                resourceId = Guid.NewGuid().ToString();
            }

            var location = entry != null
                ? $"{entry.TypeName}/{resourceId}"
                : null;

            responseBundle.Entry.Add(new Bundle.EntryComponent
            {
                Response = new Bundle.ResponseComponent
                {
                    Status = "201 Created",
                    Location = location,
                    LastModified = now
                }
            });
        }

        return responseBundle;
    }

    [RequiresApiKey]
    [ExportsAtnaAuditLog]
    [Consumes("application/fhir+json", "application/fhir+xml", "application/json-patch+json")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpPatch("DocumentReference/{id}")]
    public async Task<IActionResult> PatchDocument(string id, [FromBody] JsonElement json)
    {
        if (!await _featureManager.IsEnabledAsync("Fhir_PatchBundle")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        _logger.LogInformation($"{HttpContext.TraceIdentifier} - Received request to patch DocumentReference.securityLabel for id {id} from {Request.HttpContext.Connection.RemoteIpAddress}");
        var operationOutcome = _fhirService.PatchBundle(id, json, out var documentEntry, out var oldSecurityLabel);

        // Atna log generation
        HttpContext.Items.Add("oldSecurityLabel", oldSecurityLabel);
        HttpContext.Items.Add("patchedDocumentEntry", documentEntry);

        // Return the updated DocumentReference

        requestTimer.Stop();

        _monitoringStatusService.ResponseTimes.Add(Constants.Xds.OperationContract.Iti65PatchAction, requestTimer.ElapsedMilliseconds);

        _logger.LogInformation($"Completed action: ITI-65 PatchBundle in {requestTimer.ElapsedMilliseconds}ms with {operationOutcome.Issue.Count} issues");

        var extrinsicObject = _registryMetadataTransformerService.TransformRegistryObjectDtoToRegistryObject(documentEntry);

        var bundle = _xdsOnFhirTransformerService.TransformRegistryObjectsToFhirBundle([extrinsicObject]);
        var updatedDocRef = bundle?.Entry?.Select(e => e.Resource).OfType<DocumentReference>().FirstOrDefault(dr => string.Equals(dr.Id, id, StringComparison.OrdinalIgnoreCase))
                            ?? bundle?.Entry?.Select(e => e.Resource).OfType<DocumentReference>().FirstOrDefault();

        if (operationOutcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueSeverity.Fatal))
        {
            return BadRequestOperationOutcome.Create(operationOutcome);
        }

        if (updatedDocRef == null)
        {
            return OkOperationOutcome.Create(OperationOutcome.ForMessage($"Updated securityLabel for DocumentReference/{id}", OperationOutcome.IssueType.Success, OperationOutcome.IssueSeverity.Information));
        }

        var serializer = new FhirJsonSerializer();
        return new CustomContentResult(serializer.SerializeToString(updatedDocRef), StatusCodes.Status200OK, Constants.MimeTypes.FhirJson);
    }
}