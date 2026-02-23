using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Web;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.DataManipulators;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.Helpers;
using XcaXds.WebService.Models.Custom;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("R4/fhir")]
[Tags("FHIR Endpoints")]
[UsePolicyEnforcementPoint]
public class FhirMobileAccessToHealthDocumentsController : Controller
{
    private readonly ILogger<FhirMobileAccessToHealthDocumentsController> _logger;

    private readonly XdsRegistryService _xdsRegistryService;
    private readonly XdsRepositoryService _xdsRepositoryService;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly XdsOnFhirService _xdsOnFhirService;
    private readonly FhirService _fhirService;
    private readonly ApplicationConfig _appConfig;
    private readonly AtnaLogGeneratorService _atnaLoggingService;


    private const string HomeCommunityIdUrl_IheProfiles = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId";
    private const string HomeCommunityIdUrl_IheLegacy = "http://ihe.net/fhir/StructureDefinition/homeCommunityId";

    public FhirMobileAccessToHealthDocumentsController(
        ILogger<FhirMobileAccessToHealthDocumentsController> logger,
        XdsRegistryService xdsRegistryService,
        XdsRepositoryService xdsRepositoryService,
        RestfulRegistryRepositoryService restfulRegistryService,
        XdsOnFhirService xdsOnFhirService,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper,
        ApplicationConfig applicationConfig,
        AtnaLogGeneratorService atnaLoggingService
        )
    {
        _xdsRegistryService = xdsRegistryService;
        _xdsRepositoryService = xdsRepositoryService;
        _xdsOnFhirService = xdsOnFhirService;
        _logger = logger;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _appConfig = applicationConfig;
        _atnaLoggingService = atnaLoggingService;
        _restfulRegistryService = restfulRegistryService;
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

        if (operationOutcome.Issue.Count != 0)
        {
            requestTimer.Stop();
            _logger.LogInformation($"Completed action: ITI-67 in {requestTimer.ElapsedMilliseconds} ms with {operationOutcome.Issue.Count} errors");
            return BadRequest(fhirJsonSerializer.SerializeToString(operationOutcome));
        }

        var adhocQueryRequest = new AdhocQueryRequest
        {
            AdhocQuery = _xdsOnFhirService.ConvertIti67ToIti18AdhocQuery(documentRequest).AdhocQuery,
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

        var response = _xdsRegistryService.RegistryStoredQuery(soapEnvelope);

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
        return BadRequestOperationOutcome.Create(operationOutcome);
    }

    [HttpGet("mhd/document")]
    public async Task<IActionResult> Document(
        [FromQuery] string homeCommunityId,
        [FromQuery] string repositoryUniqueId,
        [FromQuery] string documentUniqueId
        )
    {
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"{HttpContext.TraceIdentifier} - Received request for action: ITI-68 from {Request.HttpContext.Connection.RemoteIpAddress}");

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
    [HttpDelete("DocumentReference/{id}")]
    public IActionResult DeleteDocument(string id)
    {
        _logger.LogInformation($"{HttpContext.TraceIdentifier} - Received request to delete document with id {id} from {Request.HttpContext.Connection.RemoteIpAddress}");
        var operationOutcome = new OperationOutcome();

        var deleteResponse = _restfulRegistryService.DeleteDocumentAndMetadata(id, out var deletedEntry);

        // Atna log generation
        XacmlContextRequest? xacmlRequest = null;

        if (HttpContext.Items.TryGetValue("xacmlRequest", out var xamlContextRequestObject) && xamlContextRequestObject is XacmlContextRequest xacmlContextRequest)
        {
            xacmlRequest = xacmlContextRequest;
        }

        var token = JwtExtractor.ExtractJwt(HttpContext.Request.Headers, out var ok);

        _atnaLoggingService.CreateAuditLogForFhirDeleteDocumentsRequest(HttpContext.TraceIdentifier, deletedEntry, token);

        if (deleteResponse.Success)
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Information,
                Code = OperationOutcome.IssueType.Success,
                Diagnostics = $"Document was successfully deleted from the Document Repository"
            });
        }
        else
        {
            foreach (var ooc in deleteResponse.Errors)
            {
                operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Value,
                    Diagnostics = $"{ooc.Code}: {ooc.Message}"
                });
            }
            return BadRequestOperationOutcome.Create(operationOutcome);
        }

        return OkOperationOutcome.Create(operationOutcome);
    }

    //[RequestSizeLimit(Program.OneHundredMb)] // Can be used to override options.Limits.MaxRequestBodySize in Program.cs
    [Consumes("application/fhir+json", "application/fhir+xml")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpPost("Bundle")]
    public async Task<IActionResult> ProvideBundle([FromBody] JsonElement json)
    {
        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Received request for action: ITI-65 ProvideBundle from {Request.HttpContext.Connection.RemoteIpAddress}");
        var operationOutcome = new OperationOutcome();

        var fhirJsonDeserializer = new FhirJsonDeserializer();

        var fhirParser = new FhirJsonDeserializer();
        var resource = fhirParser.DeserializeResource(json.GetRawText());

        if (resource is not Bundle fhirBundle)
        {
            return BadRequestOperationOutcome.Create(OperationOutcome.ForMessage($"Request body does not contain a well formatted FHIR bundle",
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

        //if (submissionSetList == null) return BadRequest(OperationOutcome.ForMessage($"List element is missing or malformed", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        //if (documentReferences.Count == 0) return BadRequest(OperationOutcome.ForMessage($"DocumentReference element is missing or malformed", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        //if (fhirBinaries.Count == 0) return BadRequest(OperationOutcome.ForMessage($"Binary element is missing or malformed", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        //if (patient == null) return BadRequest(OperationOutcome.ForMessage($"Patient not found in DocumentReference", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Fatal));

        if (submissionSetList == null) operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
        {
            Severity = OperationOutcome.IssueSeverity.Fatal,
            Code = OperationOutcome.IssueType.Invalid,
            Diagnostics = $"List element is missing or malformed"
        });

        if (documentReferences.Count == 0) operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
        {
            Severity = OperationOutcome.IssueSeverity.Fatal,
            Code = OperationOutcome.IssueType.Invalid,
            Diagnostics = $"DocumentReference element is missing or malformed"

        });

        if (fhirBinaries.Count == 0) operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
        {
            Severity = OperationOutcome.IssueSeverity.Fatal,
            Code = OperationOutcome.IssueType.Invalid,
            Diagnostics = $"Binary element is missing or malformed"

        });

        if (patient == null) operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
        {
            Severity = OperationOutcome.IssueSeverity.Fatal,
            Code = OperationOutcome.IssueType.Invalid,
            Diagnostics = $"Patient not found in DocumentReference"
        });

        var identifier = patient.Identifier.First();

        var patientIdCodeSystem = identifier.System?.NoUrn();
        var patientIdentifier = identifier.Value;

        var sourceIdIdentifier = submissionSetList.GetExtension("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-sourceId");
        var extResReference = sourceIdIdentifier!.Value as Identifier; // Changed from reference to identifier
        var sourceId = extResReference?.Value?.Replace("urn:oid:", "");
        //var sourceId = sourceIdIdentifier?.ElementId;

        if (string.IsNullOrEmpty(sourceId))
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = "Missing List.extension[ihe-sourceId]"
            });
        }

        var firstDocumentReference = documentReferences.First();
        var attachment = firstDocumentReference!.Content.FirstOrDefault()?.Attachment;
        var allAttachments = documentReferences.SelectMany(dr => dr!.Content).Select(c => c.Attachment).ToList();

        var homeCommunityIdExtension = attachment.GetExtension(HomeCommunityIdUrl_IheProfiles) ??
           attachment.GetExtension(HomeCommunityIdUrl_IheLegacy);

        var homeCommunityId = homeCommunityIdExtension?.Value?.ToString();

        if (string.IsNullOrEmpty(homeCommunityId))
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = "Missing DocumentReference.content.attachment.extension[homeCommunityId]"
            });
        }


        _logger.LogInformation($"{HttpContext.TraceIdentifier} Converting FHIR bundle to XDS RegistryObjectList...");
        var provideAndRegisterResult = FhirXdsTransformerService.CreateSoapObjectFromComprehensiveBundle(fhirBundle, patient, documentReferences, submissionSetList, fhirBinaries, identifier, patientIdCodeSystem?.NoUrn(), homeCommunityId.NoUrn());

        _logger.LogInformation($"{HttpContext.TraceIdentifier} RegistryObjectList conversion success: {provideAndRegisterResult.Success}\nErrors: {provideAndRegisterResult.OperationOutcome?.Issue.Count ?? 0}");

        var provideAndRegisterRequest = provideAndRegisterResult.Value?.ProvideAndRegisterDocumentSetRequest;

        if (provideAndRegisterResult.Success == false && provideAndRegisterResult.OperationOutcome != null && provideAndRegisterRequest != null)
        {
            operationOutcome.Issue.AddRange(provideAndRegisterResult.OperationOutcome.Issue);
        }

        var submittedDocumentsTooLarge = _xdsRepositoryService.CheckIfDocumentsAreTooLarge(provideAndRegisterRequest);

        var iti42Message = _xdsRegistryService.CopyIti41ToIti42Message(provideAndRegisterRequest);

        var repositoryDocumentExists = _xdsRepositoryService.CheckIfDocumentExistsInRepository(provideAndRegisterRequest);

        var registerDocumentSetResponse = _xdsRegistryService.AppendToRegistry(iti42Message.Value);

        var documentUploadResponse = _xdsRepositoryService.UploadContentToRepository(provideAndRegisterRequest);

        var errors = new List<RegistryErrorType>();
        errors.AddRange(submittedDocumentsTooLarge.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(iti42Message.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(repositoryDocumentExists.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(registerDocumentSetResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(documentUploadResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

        var fhirSerializer = new FhirJsonSerializer();

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier}Error while conver Error: {error.ErrorCode}\nErrorCode: {error.CodeContext}");

                operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Value,
                    Diagnostics = $"{error.ErrorCode}: {error.CodeContext}"
                });
            }
        }

        // Atna log generation
        var jwtToken = Request.Headers["Authorization"].FirstOrDefault();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Uploaded {fhirBundle.Entry.Count} Entries");

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Exporting AuditEvent for ITI-65 request");
        
        _atnaLoggingService.CreateAuditLogForSoapRequestResponse(AtnaLogEnricherService.GetMockSoapEnvelopeFromJwt(jwtToken, fhirBundle, errors, provideAndRegisterRequest.SubmitObjectsRequest.RegistryObjectList), registerDocumentSetResponse.Value);

        if (operationOutcome.Issue.Any())
        {
            return new CustomContentResult(fhirSerializer.SerializeToString(operationOutcome), StatusCodes.Status400BadRequest, Constants.MimeTypes.FhirJson);
        }

        // --- MHD ProvideDocumentBundleResponse (Bundle type = transaction-response) ---
        var selfUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

        var responseBundle = new Bundle
        {
            Id = "bundle-response-" + Guid.NewGuid().ToString(),
            Type = Bundle.BundleType.TransactionResponse,
            Meta = new Meta
            {
                Profile = new[]
                {
                    "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.ProvideDocumentBundleResponse"
                }
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

        string jsonResult;

        var options = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();

        jsonResult = JsonSerializer.Serialize(responseBundle, options);

        return Content(jsonResult, "application/json");
    }

    [Consumes("application/fhir+json")]
    [Produces("application/fhir+json", "application/fhir+xml")]
    [HttpPatch("Bundle/{id}")]
    public async Task<IActionResult> PatchBundle(string id, [FromBody] JsonElement json)
    {
        _logger.LogInformation($"{HttpContext.TraceIdentifier} Received ITI-65 Patch Bundle Request");
        var operationOutcome = new OperationOutcome();

        var fhirJsonDeserializer = new FhirJsonDeserializer();

        var fhirParser = new FhirJsonDeserializer();
        var resource = fhirParser.DeserializeResource(json.GetRawText());

        if (resource is not Bundle fhirBundle)
        {
            return BadRequestOperationOutcome.Create(OperationOutcome
                .ForMessage($"Request body does not contain a well formatted FHIR bundle",
                    OperationOutcome.IssueType.Invalid,
                    OperationOutcome.IssueSeverity.Fatal));
        }

        if (fhirBundle.Entry.Any(ent => ent.Request?.Method != Bundle.HTTPVerb.PATCH))
        {
            return BadRequestOperationOutcome.Create(OperationOutcome
                .ForMessage($"Bundle must contain a PATCH entry",
                    OperationOutcome.IssueType.Invalid,
                    OperationOutcome.IssueSeverity.Fatal));
        }

        var response = _fhirService.PatchResource(fhirBundle);

        var options = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();

        var jsonResult = JsonSerializer.Serialize(response, options);

        return Content(jsonResult, "application/json");
    }
}