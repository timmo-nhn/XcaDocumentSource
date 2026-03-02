using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DomainResults;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Services;

public class FhirService
{
    private readonly ILogger<FhirService> _logger;
    private readonly XdsRegistryService _registry;
    private readonly XdsRepositoryService _repository;


    private const string HomeCommunityIdUrl_IheProfiles = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId";
    private const string HomeCommunityIdUrl_IheLegacy = "http://ihe.net/fhir/StructureDefinition/homeCommunityId";

    public FhirService(ILogger<FhirService> logger, XdsRegistryService registry, XdsRepositoryService repository)
    {
        _logger = logger;
        _registry = registry;
        _repository = repository;
    }

    public OperationOutcome PatchResource(Bundle bundle)
    {
        foreach (var entry in bundle.Entry)
        {
            var url = entry.FullUrl;

            if (entry.Resource is not Binary fhirBinary) continue;

            var patchData = Encoding.UTF8.GetString(fhirBinary.Data ?? []);

        }


        return new OperationOutcome();
    }

    public ProvideBundleResult ProvideBundle(Bundle fhirBundle, string sessionId, bool validateOnly = false)
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

        var identifier = patient?.Identifier.First();

        var patientIdCodeSystem = identifier?.System?.NoUrn();

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

        _logger.LogInformation($"{sessionId} Converting FHIR bundle to XDS RegistryObjectList...");
        var provideAndRegisterResult = FhirToXdsTransformer.CreateSoapObjectFromComprehensiveBundle(fhirBundle, patient, documentReferences, submissionSetList, fhirBinaries, identifier, patientIdCodeSystem?.NoUrn(), homeCommunityId.NoUrn());

        _logger.LogInformation($"{sessionId} RegistryObjectList conversion success: {provideAndRegisterResult.Success}\nErrors: {provideAndRegisterResult.OperationOutcome?.Issue.Count ?? 0}");

        var provideAndRegisterRequest = provideAndRegisterResult.Value?.ProvideAndRegisterDocumentSetRequest;

        if (provideAndRegisterResult.Success == false && provideAndRegisterResult.OperationOutcome != null && provideAndRegisterRequest != null)
        {
            operationOutcome.Issue.AddRange(provideAndRegisterResult.OperationOutcome.Issue);
        }

        var submittedDocumentsTooLarge = _repository.CheckIfDocumentsAreTooLarge(provideAndRegisterRequest);

        var iti42SoapEnvelope = _registry.CopyIti41ToIti42Message(provideAndRegisterRequest);

        var repositoryDocumentExists = _repository.CheckIfDocumentExistsInRepository(provideAndRegisterRequest);

        SoapRequestResult<SoapEnvelope>? registerDocumentSetResponse = null;
        SoapRequestResult<SoapEnvelope>? documentUploadResponse = null;

        // If operation is $validate, we only want to validate the request without actually registering/uploading the documents.
        // https://build.fhir.org/resource-operation-validate.html
        if (validateOnly == true)
        {
            registerDocumentSetResponse = _registry.AppendToRegistry(iti42SoapEnvelope.Value);
            documentUploadResponse = _repository.UploadContentToRepository(provideAndRegisterRequest);
        }

        var errors = new List<RegistryErrorType>();
        errors.AddRange(submittedDocumentsTooLarge.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(iti42SoapEnvelope.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        errors.AddRange(repositoryDocumentExists.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

        if (registerDocumentSetResponse != null && documentUploadResponse != null)
        {
            errors.AddRange(registerDocumentSetResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
            errors.AddRange(documentUploadResponse.Value?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);
        }


        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                _logger.LogError($"{sessionId}Error while converting to Bundle\n\tError: {error.ErrorCode}\n\tErrorCode: {error.CodeContext}");

                operationOutcome.Issue.Add(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Value,
                    Diagnostics = $"{error.ErrorCode}: {error.CodeContext}"
                });
            }
        }

        return new()
        {
            Outcome = operationOutcome,
            ProvideAndRegisterRequest = provideAndRegisterRequest,
            RegistryResponse = registerDocumentSetResponse?.Value,
            Errors = errors,
        };
    }
}