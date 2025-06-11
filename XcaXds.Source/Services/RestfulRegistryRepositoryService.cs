using Microsoft.Extensions.Logging;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;

namespace XcaXds.Source.Services;

public class RestfulRegistryRepositoryService
{
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<XdsRegistryService> _logger;
    private readonly RegistryMetadataTransformerService _metadataTransformerService;
    private readonly RepositoryWrapper _repositoryWrapper;

    public RestfulRegistryRepositoryService(ApplicationConfig appConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper, ILogger<XdsRegistryService> logger, RegistryMetadataTransformerService metadataTransformerService, RepositoryWrapper repositoryWrapper)
    {
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _metadataTransformerService = metadataTransformerService;
    }

    public DocumentListResponse GetDocumentListForPatient(string? patientId, string? status)
    {
        var documentListResponse = new DocumentListResponse();

        if (string.IsNullOrWhiteSpace(patientId))
        {
            documentListResponse.AddError("RequiredParameterMissing", "Parameter 'id' is required.");
            return documentListResponse;
        }

        var allowedStatuses = new[] { "Approved", "Deprecated" };

        if (!string.IsNullOrWhiteSpace(status) && !allowedStatuses.Contains(status))
        {
            documentListResponse.AddError("RequiredParameterMissing", @$"Status must be one of ""Approved"" or ""Deprecated"", got {status}.");
            return documentListResponse;

        }

        var patientIdCx = Hl7Object.Parse<CX>(patientId);

        // Account for searches only including the patient Id and not assigning authority (eg api/GetDocumentList?id=13116900216)
        // Add default assigning authority if missing
        patientIdCx.AssigningAuthority ??= new() { UniversalId = Constants.Oid.Fnr, UniversalIdType = Constants.Hl7.UniversalIdType.Iso };

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var patientDocumentReferences = documentRegistry
            .OfType<DocumentEntryDto>()
            .ByDocumentEntryPatientId(patientIdCx);

        documentListResponse.DocumentListEntries = patientDocumentReferences
            .Select(dr => new DocumentListEntry()
            {
                DocumentReference = dr,
                LinkToDocument = new()
                {
                    Title = dr.Title,
                    Url = $"/document?home={dr.HomeCommunityId}&repository={dr.RepositoryUniqueId}&document={dr.UniqueId}"
                }
            }).ToList();

        return documentListResponse;
    }

    public DocumentResponse GetDocument(string? home, string? repository, string? document)
    {
        var documentResponse = new DocumentResponse();


        if (string.IsNullOrWhiteSpace(home) || string.IsNullOrWhiteSpace(repository) || string.IsNullOrWhiteSpace(document))
        {
            documentResponse.AddError("RequiredParameterMissing", "Parameters homecommunity_id, repository_id, document_id are required.");
            return documentResponse;
        }

        var documentBytes = _repositoryWrapper.GetDocumentFromRepository(home, repository, document);

        if (documentBytes?.Length > 0)
        {
            documentResponse.Document = new()
            {
                Data = documentBytes,
                DocumentId = document
            };
        }

        return documentResponse;
    }

    public RestfulApiResponse UploadDocumentAndMetadata(DocumentReferenceDto documentReference)
    {
        var uploadResponse = new RestfulApiResponse();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var elementsToBeUploaded = new List<RegistryObjectDto>() { documentReference.DocumentEntry, documentReference.SubmissionSet, documentReference.Association };

        if (DuplicateUuidsExist(documentRegistry, elementsToBeUploaded, out var dupliacteIds))
        {
            uploadResponse.AddError("UniqueIdError", $"Duplicate identifiers found in submission and registry: {string.Join(", ", dupliacteIds)}");
            return uploadResponse;
        }

        _registryWrapper.UpdateDocumentRegistryContentWithDtos(elementsToBeUploaded);

        return uploadResponse;
    }

    private bool DuplicateUuidsExist(List<RegistryObjectDto> registryObjectList, List<RegistryObjectDto> submissionRegistryObjects, out string[] duplicateIds)
    {
        var allObjects = registryObjectList.Concat(submissionRegistryObjects);

        var duplicates = allObjects
            .Where(obj => obj.Id != null)
            .GroupBy(obj => obj.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        duplicateIds = duplicates;

        return duplicateIds.Length > 0;
    }

    public void UpdateDocumentMetadata(DocumentReferenceDto value)
    {
        throw new NotImplementedException();
    }

    public void DeleteDocumentAndMetadata(object value)
    {
        throw new NotImplementedException();
    }
}
