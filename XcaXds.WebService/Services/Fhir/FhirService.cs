using Hl7.Fhir.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DomainResults;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Services.Fhir;

public class FhirService
{
    private readonly ILogger<FhirService> _logger;
    private readonly RegistryWrapper _registryWrapper;
    private readonly XdsRegistryService _registry;
    private readonly XdsRepositoryService _repository;
    private readonly FhirToXdsTransformerService _fhirToXdsTransformerService;

    private const string HomeCommunityIdUrl_IheProfiles = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId";
    private const string HomeCommunityIdUrl_IheLegacy = "http://ihe.net/fhir/StructureDefinition/homeCommunityId";

    public FhirService(
        ILogger<FhirService> logger,
        RegistryWrapper registryWrapper,
        XdsRegistryService registry,
        XdsRepositoryService repository,
        FhirToXdsTransformerService fhirToXdsTransformerService)
    {
        _logger = logger;
        _registryWrapper = registryWrapper;
        _registry = registry;
        _repository = repository;
        _fhirToXdsTransformerService = fhirToXdsTransformerService;
    }

    public async Task<ProvideBundleResult> ProvideBundle(Bundle fhirBundle, string sessionId, bool validateOnly = false)
    {
        var operationOutcome = new OperationOutcome();

        var patient = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<Patient>()
            .FirstOrDefault();

        var documentReferences = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<DocumentReference>()
            .ToList();

        var fhirBinaries = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<Binary>()
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

        _logger.LogInformation("{traceIdentifier} Converting FHIR bundle to XDS RegistryObjectList...", sessionId);
        var provideAndRegisterResult = _fhirToXdsTransformerService.CreateSoapObjectFromComprehensiveBundle(fhirBundle, patient, documentReferences, submissionSetList, fhirBinaries, homeCommunityId?.NoUrn());

        _logger.LogInformation("{traceIdentifier} RegistryObjectList conversion success: {success}\nErrors: {errorCount}", sessionId, provideAndRegisterResult.Success, provideAndRegisterResult.OperationOutcome?.Issue.Count ?? 0);

        var provideAndRegisterRequest = provideAndRegisterResult.Value;

        if (provideAndRegisterResult.Success == false && provideAndRegisterResult.OperationOutcome != null && provideAndRegisterRequest != null)
        {
            operationOutcome.Issue.AddRange(provideAndRegisterResult.OperationOutcome.Issue);
        }

        var submittedDocumentsTooLarge = _repository.CheckIfDocumentsAreTooLarge(provideAndRegisterRequest);

        var iti42SoapEnvelope = _registry.CopyIti41ToIti42Message(provideAndRegisterRequest);

        var repositoryDocumentExists = _repository.CheckIfDocumentExistsInRepository(provideAndRegisterRequest);

        SoapRequestResult<SoapEnvelope>? registerDocumentSetResponse = null;
        SoapRequestResult<SoapEnvelope>? documentUploadResponse = null;

        // Any errors that have previously occured will override the validateOnly parameter, supporting
        // the atomic nature of provide and register (all-or-nothing),
        // while also allowing us to validate everything without returning too early, preventing
        // the end user from fighting "waves" of errors
        var effectiveValidateOnly =
            repositoryDocumentExists.IsSuccess == false ||
            iti42SoapEnvelope.IsSuccess == false ||
            submittedDocumentsTooLarge.IsSuccess == false ||
            validateOnly;

        documentUploadResponse = await _repository.UploadContentToRepository(provideAndRegisterRequest, effectiveValidateOnly);

        effectiveValidateOnly = documentUploadResponse.IsSuccess == false;

        registerDocumentSetResponse = _registry.AppendToRegistry(iti42SoapEnvelope.Value, effectiveValidateOnly);


        var errors = new List<RegistryErrorType>();
        errors.AddRange(submittedDocumentsTooLarge.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(iti42SoapEnvelope.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(repositoryDocumentExists.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

        if (registerDocumentSetResponse != null && documentUploadResponse != null)
        {
            errors.AddRange(registerDocumentSetResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
            errors.AddRange(documentUploadResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        }

        var highestSeverity = errors.MaxBy(err => err.GetSeverityLevel())?.Severity;

        registerDocumentSetResponse?.Value?.Body.RegistryResponse = new()
        {

            RegistryErrorList = new()
            {
                HighestSeverity = highestSeverity,
                RegistryError = errors.ToArray()
            }
        };

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                _logger.LogError("{traceIdentifier} Error while converting to Bundle\n\tError: {errorCode}\n\tErrorCode: {codeContext}", sessionId, error.ErrorCode, error.CodeContext);

                operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Value,
                    Diagnostics = $"XDSError_Code: {error.ErrorCode}, XDSError_CodeContext: {error.CodeContext}"
                });
            }
        }

        return new()
        {
            Outcome = operationOutcome,
            ProvideAndRegisterRequest = provideAndRegisterRequest,
            RegistryResponse = registerDocumentSetResponse?.Value
        };
    }


    public OperationOutcome PatchBundle(string id, JsonElement json, out DocumentEntryDto documentEntry, out CodedValue[] oldCodes)
    {
        documentEntry = null!;
        oldCodes = null!;

        var operationOutcome = new OperationOutcome();
        if (string.IsNullOrWhiteSpace(id))
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = "Missing id"
            });
        }

        if (!TryExtractSecurityLabelElement(json, out var securityLabelElement, out var errorMessage))
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = errorMessage ?? "Missing securityLabel"
            });
        }

        var fetchedDocumentEntry = _registryWrapper.GetSingleRegistryObjectAsDto(id) as DocumentEntryDto;

        if (fetchedDocumentEntry == null)
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = $"DocumentReference/{id} not found"
            });
        }

        var codings = ParseSecurityLabelToCodedValues(securityLabelElement, out var parseError);
        if (codings == null)
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = parseError ?? "Invalid securityLabel"
            });
        }
        if (codings != null)
        {

            var results = ValidateIncomingPatchBundleCodings(codings);

            if (results.Count > 0)
            {
                operationOutcome.Issue.AddRange(results.Select(res => new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Invalid,
                    Diagnostics = res.ErrorMessage
                }));
            }
        }

        var anyErrors = operationOutcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueSeverity.Fatal);

        if (anyErrors)
            return operationOutcome;

        var oldSecurityLabel = fetchedDocumentEntry!.ConfidentialityCode == null
            ? null
            : fetchedDocumentEntry.ConfidentialityCode.Select(c => new CodedValue
            {
                Code = c.Code,
                CodeSystem = c.CodeSystem,
                DisplayName = c.DisplayName
            }).ToList();


        fetchedDocumentEntry.ConfidentialityCode = codings;

        var response = _registryWrapper.InsertOrUpdateDocumentRegistryContentWithDtos(fetchedDocumentEntry);

        documentEntry = fetchedDocumentEntry;
        oldCodes = [.. oldSecurityLabel ?? []];

        if (!response.IsSuccess)
        {
            operationOutcome.Issue.Add(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = $"Failed to update DocumentReference/{id}: {response.Message}",
            });
        }

        return operationOutcome;
    }

    private static bool TryExtractSecurityLabelElement(JsonElement json, out JsonElement securityLabelElement, out string? errorMessage)
    {
        securityLabelElement = default;
        errorMessage = null;

        if (json.ValueKind == JsonValueKind.Array)
        {
            // JSON Patch (RFC 6902): [{"op":"replace","path":"/securityLabel","value":[...]}]
            foreach (var op in json.EnumerateArray())
            {
                if (op.ValueKind != JsonValueKind.Object) continue;
                var hasOp = TryGetPropertyCaseInsensitive(op, "op", out var opName);
                var hasPath = TryGetPropertyCaseInsensitive(op, "path", out var path);
                if (!hasOp || !hasPath) continue;

                var opNameValue = opName.GetString();
                var pathValue = path.GetString();
                if (string.IsNullOrWhiteSpace(opNameValue) || string.IsNullOrWhiteSpace(pathValue)) continue;

                if (!pathValue.Equals("/securityLabel", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "Only /securityLabel can be patched";
                    return false;
                }

                if (!string.Equals(opNameValue, "add", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(opNameValue, "replace", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "Only add/replace operations are supported";
                    return false;
                }

                if (!TryGetPropertyCaseInsensitive(op, "value", out var value))
                {
                    errorMessage = "Patch operation is missing value";
                    return false;
                }

                securityLabelElement = value;
                return true;
            }

            errorMessage = "Invalid JSON Patch payload";
            return false;
        }

        if (json.ValueKind != JsonValueKind.Object)
        {
            errorMessage = "Body must be a JSON object or JSON Patch array";
            return false;
        }

        // Partial resource style: { "securityLabel": [ ... ] }
        if (!TryGetPropertyCaseInsensitive(json, "securityLabel", out securityLabelElement))
        {
            errorMessage = "Body must include securityLabel";
            return false;
        }

        // Whitelist (for future expansion, add more here)
        foreach (var prop in json.EnumerateObject())
        {
            if (prop.NameEquals("securityLabel") || prop.NameEquals("resourceType") || prop.NameEquals("id"))
                continue;
            errorMessage = $"Property '{prop.Name}' is not allowed to be patched";
            return false;
        }

        return true;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement obj, string name, out JsonElement value)
    {
        if (obj.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in obj.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static List<CodedValue>? ParseSecurityLabelToCodedValues(JsonElement securityLabelElement, out string? errorMessage)
    {
        errorMessage = null;
        if (securityLabelElement.ValueKind != JsonValueKind.Array)
        {
            errorMessage = "securityLabel must be an array";
            return null;
        }

        var result = new List<CodedValue>();
        foreach (var label in securityLabelElement.EnumerateArray())
        {
            if (label.ValueKind != JsonValueKind.Object)
            {
                errorMessage = "Each securityLabel entry must be an object";
                return null;
            }

            if (!TryGetPropertyCaseInsensitive(label, "coding", out var codingElement) || codingElement.ValueKind != JsonValueKind.Array)
            {
                errorMessage = "Each securityLabel entry must include coding[]";
                return null;
            }

            var addedAny = false;
            foreach (var coding in codingElement.EnumerateArray())
            {
                if (coding.ValueKind != JsonValueKind.Object)
                {
                    errorMessage = "securityLabel.coding must contain objects";
                    return null;
                }

                string? code = null;
                string? system = null;
                string? display = null;

                if (TryGetPropertyCaseInsensitive(coding, "code", out var codeEl))
                {
                    code = codeEl.GetString();
                }

                if (TryGetPropertyCaseInsensitive(coding, "system", out var sysEl))
                {
                    system = sysEl.GetString();
                }

                if (TryGetPropertyCaseInsensitive(coding, "display", out var dispEl))
                {
                    display = dispEl.GetString();
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    errorMessage = "securityLabel.coding.code is required";
                    return null;
                }

                result.Add(new CodedValue
                {
                    Code = code,
                    CodeSystem = string.IsNullOrWhiteSpace(system) ? null : system.NoUrn(),
                    DisplayName = display
                });
                addedAny = true;
            }

            if (!addedAny)
            {
                errorMessage = "securityLabel.coding must contain at least one object";
                return null;
            }
        }

        return result;
    }

    private List<ValidationResult> ValidateIncomingPatchBundleCodings(List<CodedValue> codings)
    {
        var results = new List<ValidationResult>();
        foreach (var coding in codings)
        {
            var context = new ValidationContext(coding);
            Validator.TryValidateObject(coding, context, results, true);
        }

        return results;
    }
}