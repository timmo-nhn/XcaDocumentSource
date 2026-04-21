using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;

namespace XcaXds.WebService.Services;

public class RestfulRegistryRepositoryService
{
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<XdsRegistryService> _logger;
    private readonly RepositoryWrapper _repositoryWrapper;

    public RestfulRegistryRepositoryService(ApplicationConfig appConfig, RegistryWrapper registryWrapper, ILogger<XdsRegistryService> logger, RepositoryWrapper repositoryWrapper)
    {
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public DocumentListResponse GetDocumentListForPatient(string? patientId, string? status, DateTime? serviceStartTime = null, DateTime? serviceStopTime = null, int currentPageNumber = 1, int pageSize = 10)
    {
        var documentListResponse = new DocumentListResponse();

        if (string.IsNullOrWhiteSpace(patientId))
        {
            documentListResponse.AddError("MissingParameter", "Parameter 'id' is required.");
            return documentListResponse;
        }

        var allowedStatuses = new[] { "approved", "deprecated" };

        if (!string.IsNullOrWhiteSpace(status) && !allowedStatuses.Contains(status))
        {
            documentListResponse.AddError("MissingParameter", @$"Status must be one of ""approved"" or ""deprecated"", got {status}.");
            return documentListResponse;
        }

        var patientIdCx = Hl7Object.Parse<CX>(patientId)!;

        // Account for searches only including the patient Id and not assigning authority (eg api/GetDocumentList?id=13116900216)
        // Add default assigning authority if missing
        patientIdCx.AssigningAuthority ??= Hl7FhirExtensions.ParseNorwegianNinToCxWithAssigningAuthority(patientId)?.AssigningAuthority ?? new() { UniversalId = Constants.Oid.Fnr, UniversalIdType = Constants.Hl7.UniversalIdType.Iso }; ;

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var patientDocumentReferences = documentRegistry
            .OfType<DocumentEntryDto>()
            .ByDocumentEntryPatientId(patientIdCx)
            .ByDocumentEntryStatus(status)
            .ByDocumentEntryServiceStartTime(serviceStartTime)
            .ByDocumentEntryServiceStopTime(serviceStopTime)
            .ToList();

        var paginatedDocumentList = patientDocumentReferences
            .Skip((currentPageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var documentListEntriesWithLink = paginatedDocumentList
            .Select(dr => new DocumentListEntry()
            {
                DocumentReference = dr,
                LinkToDocument = new()
                {
                    Title = dr.Title,
                    Url = $"api/rest/document?home={dr.HomeCommunityId}&repository={dr.RepositoryUniqueId}&document={dr.UniqueId}"
                }
            }).ToList();


        var totalPages = (int)Math.Ceiling((double)patientDocumentReferences.Count / pageSize);

        var pagination = new Pagination();
        if (currentPageNumber < 1 || currentPageNumber - 1 > totalPages)
        {
            pagination = new Pagination()
            {
                TotalResults = patientDocumentReferences.Count,
                NumberOfResults = 0,
                PageNumber = currentPageNumber,
                Next = null,
                Prev = null,
                LastPage = totalPages
            };
        }
        else
        {
            var nextPageNull = currentPageNumber >= totalPages;
            var previousPageNull = currentPageNumber <= 1;

            pagination = new Pagination()
            {
                TotalResults = patientDocumentReferences.Count,
                NumberOfResults = paginatedDocumentList.Count,
                PageNumber = currentPageNumber,
                Next = nextPageNull ? null : currentPageNumber + 1,
                Prev = previousPageNull ? null : currentPageNumber - 1,
                LastPage = totalPages
            };
        }

        documentListResponse.Pagination = pagination;
        documentListResponse.DocumentListEntries = documentListEntriesWithLink;

        return documentListResponse;
    }

    public DocumentResponse GetDocument(string? home, string? repository, string? document)
    {
        var documentResponse = new DocumentResponse();

        home ??= _appConfig.HomeCommunityId;
        repository ??= _appConfig.RepositoryUniqueId;

        if (string.IsNullOrWhiteSpace(document))
        {
            documentResponse.AddError("MissingParameter", "Parameter document is required.");
            return documentResponse;
        }

        var documentBytes = _repositoryWrapper.GetDocumentFromRepository(home, repository, document, out _);

        if (documentBytes?.Length > 0)
        {
            documentResponse.Document = new()
            {
                Data = documentBytes,
                DocumentId = document
            };
        }
        else
        {
            documentResponse.SetMessage($"No document with id {document} for home {home}, repository {repository}");
        }

        return documentResponse;
    }

    public DocumentStatusResponse GetDocumentStatus(string? home, string? repository, string? document)
    {
        home ??= _appConfig.HomeCommunityId;
        repository ??= _appConfig.RepositoryUniqueId;

        var documentStatusResponse = new DocumentStatusResponse()
        {
            Document = new DocumentStatusDto()
            {
                HomeCommunityId = home,
                RepositoryUniqueId = repository,
                DocumentId = document,
            }
        };

        if (string.IsNullOrWhiteSpace(document))
        {
            documentStatusResponse.AddError("MissingParameter", "Parameter document is required.");
            return documentStatusResponse;
        }

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntry = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(docEnt => docEnt.Id == document);

        if (documentEntry != null)
        {
            documentStatusResponse.Document.SavedToRegistry = true;
        }

        var documentBytes = _repositoryWrapper.GetDocumentFromRepository(home, repository, document, out _);

        if (documentBytes?.Length > 0)
        {
            documentStatusResponse.Document.SavedToRepository = true;
        }
        else
        {
            documentStatusResponse.SetMessage($"No document with id {document} for home {home}, repository {repository}");
        }

        documentStatusResponse.Document.IsFullySaved = documentStatusResponse.Document.SavedToRegistry && documentStatusResponse.Document.SavedToRepository;

        return documentStatusResponse;
    }

    public RestfulApiResponse UploadDocumentAndMetadata(DocumentReferenceDto documentReference)
    {
        var uploadResponse = new RestfulApiResponse();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        if (documentReference.Association == null)
        {
            documentReference.Association = CreateAssociationBetweenObjects(documentReference.SubmissionSet, documentReference.DocumentEntry);
        }

        var elementsToBeUploaded = new List<RegistryObjectDto?>()
        {
            documentReference.DocumentEntry,
            documentReference.SubmissionSet,
            documentReference.Association
        }.OfType<RegistryObjectDto>().ToList();

        if (DuplicateUuidsExist(documentRegistry, elementsToBeUploaded, out var dupliacteIds))
        {
            uploadResponse.AddError("UniqueIdError", $"Duplicate identifiers found in submission and registry: {string.Join(", ", dupliacteIds)}");
            return uploadResponse;
        }

        var doc = documentReference.Document;
        var patientId = documentReference.DocumentEntry?.SourcePatientInfo?.PatientId?.Id;

        if (doc?.DocumentId == null || doc.Data == null || patientId == null)
        {
            uploadResponse.AddError("MissingValue", "Document, DocumentId, Document Data and PatientId are required for upload");
            return uploadResponse;
        }

        _repositoryWrapper.StoreDocument(doc.DocumentId, doc.Data, patientId);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(elementsToBeUploaded);

        return uploadResponse;
    }

    public UpdateResponse UpdateDocumentMetadata(bool? replace, DocumentReferenceDto inputDocumentReference)
    {
        var updateResponse = new UpdateResponse();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntryToBeReplaced = documentRegistry
            .OfType<DocumentEntryDto>()
            .FirstOrDefault(ss => ss.Id == inputDocumentReference.DocumentEntry?.Id);

        var submissionSetToBeReplaced = documentRegistry
            .OfType<SubmissionSetDto>()
            .FirstOrDefault(ss => ss.Id == inputDocumentReference.SubmissionSet?.Id);

        if (documentEntryToBeReplaced == null || submissionSetToBeReplaced == null)
        {
            if (documentEntryToBeReplaced == null)
            {
                updateResponse.AddError("UploadError", $"No document entry with ID {inputDocumentReference.DocumentEntry?.Id}");
            }

            if (submissionSetToBeReplaced == null)
            {
                updateResponse.AddError("UploadError", $"No submission set with Id {inputDocumentReference.SubmissionSet?.Id}");
            }

            return updateResponse;
        }

        if (replace == true)
        {
            if (inputDocumentReference.DocumentEntry != null)
            {
                documentRegistry = documentRegistry.Replace(documentEntryToBeReplaced, inputDocumentReference.DocumentEntry).ToList();
            }

            if (inputDocumentReference.SubmissionSet != null)
            {
                documentRegistry = documentRegistry.Replace(submissionSetToBeReplaced, inputDocumentReference.SubmissionSet).ToList();
            }

            _registryWrapper.SetDocumentRegistryContentWithDtos(documentRegistry.ToList());

            if (inputDocumentReference.Document != null && inputDocumentReference.Document.DocumentId != null && inputDocumentReference.Document.Data?.Length > 0 && inputDocumentReference.DocumentEntry?.SourcePatientInfo?.PatientId?.Id != null)
            {
                var storeResult = _repositoryWrapper.StoreDocument(inputDocumentReference.Document.DocumentId, inputDocumentReference.Document.Data, inputDocumentReference.DocumentEntry.SourcePatientInfo.PatientId.Id);
                if (storeResult == false)
                {
                    updateResponse.AddError("UploadError", "Error while uploading document");
                }
            }

            updateResponse.SetMessage($"DocumentEntry {documentEntryToBeReplaced.Id} updated with new DocumentEntry");

        }
        else
        {
            // Create new identifiers
            var documentEntryId = Guid.NewGuid().ToString();
            _logger.LogInformation($"REPLACE: \nDocumentEntry new ID: {documentEntryId} \nPrevious: {inputDocumentReference.DocumentEntry?.Id}");
            inputDocumentReference.DocumentEntry?.Id = documentEntryId;


            var submissionSetId = Guid.NewGuid().ToString();
            _logger.LogInformation($"REPLACE: \nSubmissionSet new ID: {submissionSetId} \nPrevious: {inputDocumentReference.SubmissionSet?.Id}");
            inputDocumentReference.SubmissionSet?.Id = submissionSetId;

            // Deprecate the old DocumentEntry
            documentRegistry = documentRegistry.DeprecateEntry(documentEntryToBeReplaced.Id);

            // Create RPLC association between old and new DocumentEntry
            var replaceAssociation = CreateAssociationBetweenObjects(
                inputDocumentReference.DocumentEntry,
                documentEntryToBeReplaced,
                Constants.Xds.AssociationType.Replace);

            if (replaceAssociation == null) throw new InvalidOperationException($"Cannot create association between {inputDocumentReference.DocumentEntry?.Id} and {documentEntryToBeReplaced.Id}");
            if (inputDocumentReference.DocumentEntry == null) throw new InvalidOperationException($"DocumentEntry cannot be null");

            _logger.LogInformation($"REPLACE: \nReplace Association: {replaceAssociation.Id} Created between {inputDocumentReference.DocumentEntry.Id} and {documentEntryToBeReplaced.Id}");

            // Recreate association with new identifiers
            inputDocumentReference.Association = CreateAssociationBetweenObjects(
                inputDocumentReference.SubmissionSet,
                inputDocumentReference.DocumentEntry);

            _registryWrapper.UpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto?>()
            {
                inputDocumentReference.DocumentEntry,
                inputDocumentReference.SubmissionSet,
                inputDocumentReference.Association,
                replaceAssociation
            }.OfType<RegistryObjectDto>().ToList());

            updateResponse.SetMessage($"REPLACE: \nDocument deprecated and replaced by new DocumentEntry {inputDocumentReference.DocumentEntry.Id}");
        }


        return updateResponse;
    }

    public RestfulApiResponse PartiallyUpdateDocumentMetadata(DocumentReferenceDto value)
    {
        var partialUpdateResponse = new RestfulApiResponse();
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntryToPatch = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.Id == value.DocumentEntry?.Id);
        var submissionSetToPatch = documentRegistry.OfType<SubmissionSetDto>().FirstOrDefault(ro => ro.Id == value.SubmissionSet?.Id);
        var associationToPatch = documentRegistry.OfType<AssociationDto>().FirstOrDefault(ro => ro.Id == value.Association?.Id);

        var logSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };

        if (documentEntryToPatch != null)
        {
            _logger.LogInformation($"Updating documentEntry {documentEntryToPatch.Id} with values:\n {JsonSerializer.Serialize(value.DocumentEntry, logSerializerOptions)}");
            ObjectMerger.MergeObjects(documentEntryToPatch, value.DocumentEntry);
        }

        if (submissionSetToPatch != null)
        {
            _logger.LogInformation($"Updating submissionSet {submissionSetToPatch.Id} with values:\n {JsonSerializer.Serialize(value.SubmissionSet, logSerializerOptions)}");
            ObjectMerger.MergeObjects(submissionSetToPatch, value.SubmissionSet);
        }

        if (associationToPatch != null)
        {
            _logger.LogInformation($"Updating association {associationToPatch.Id} with values:\n {JsonSerializer.Serialize(value.Association, logSerializerOptions)}");
            ObjectMerger.MergeObjects(associationToPatch, value.Association);
        }

        var patchedResources = new List<RegistryObjectDto?>() { documentEntryToPatch, submissionSetToPatch, associationToPatch }.OfType<RegistryObjectDto>().ToList();

        _registryWrapper.UpdateDocumentRegistryContentWithDtos(patchedResources);

        return partialUpdateResponse;
    }

    public RestfulApiResponse DeleteDocumentAndMetadata(string id)
    {
        return DeleteDocumentAndMetadata(id, out _);
    }

    public RestfulApiResponse DeleteDocumentAndMetadata(string id, out DocumentEntryDto? deletedEntry)
    {
        var apiResponse = new RestfulApiResponse();
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntryForDocument = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(de => de.Id == id);

        deletedEntry = documentEntryForDocument;

        if (documentEntryForDocument == null)
        {
            _logger.LogWarning($"Error while deleting document");
            apiResponse.AddError("DeleteError", $"RegistryObject {id} not found");
        }

        if (_repositoryWrapper.FileExistsInRepository(documentEntryForDocument?.Id ?? documentEntryForDocument?.UniqueId))
        {
            var documentDeleteResponse = _repositoryWrapper.DeleteSingleDocument(documentEntryForDocument?.Id);

            if (documentDeleteResponse == false)
            {
                documentDeleteResponse = _repositoryWrapper.DeleteSingleDocument(documentEntryForDocument?.UniqueId);
            }

            if (documentDeleteResponse == false)
            {
                _logger.LogWarning($"Error while deleting document");
                apiResponse.AddError("DeleteError", $"Error while deleting document {id}");
                return apiResponse;
            }
        }

        var associationsForEntry = documentRegistry
            .OfType<AssociationDto>()
            .Where(assoc => assoc.TargetObject == id && assoc.AssociationType == Constants.Xds.AssociationType.HasMember)
            .ToList();

        var docentryCount = 0;
        var submissionSetCount = 0;
        var associationCount = 0;

        foreach (var association in associationsForEntry)
        {
            var documentEntry = documentRegistry.OfType<RegistryObjectDto>().FirstOrDefault(ss => ss.Id == association?.TargetObject);
            var submissionSet = documentRegistry.OfType<RegistryObjectDto>().FirstOrDefault(ss => ss.Id == association?.SourceObject);

            if (documentEntry != null)
            {
                _registryWrapper.DeleteDocumentEntryFromRegistry(documentEntry);
                docentryCount++;
            }
            if (submissionSet != null)
            {
                _registryWrapper.DeleteDocumentEntryFromRegistry(submissionSet);
                submissionSetCount++;
            }

            _registryWrapper.DeleteDocumentEntryFromRegistry(association);
            associationCount++;
        }

        if ((associationsForEntry.Count > 0) == false && documentEntryForDocument != null)
        {
            docentryCount++;
            _registryWrapper.DeleteDocumentEntryFromRegistry(documentEntryForDocument);
            apiResponse.SetMessage($"Successfully removed {docentryCount} DocumentEntry");
        }

        apiResponse.SetMessage($"Successfully removed {docentryCount} DocumentEntries, {submissionSetCount} SubmissisonSets and {associationCount} Associations");

        return apiResponse;
    }

    private bool DuplicateUuidsExist(IEnumerable<RegistryObjectDto> registryObjectList, List<RegistryObjectDto> submissionRegistryObjects, out string[] duplicateIds)
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

    private AssociationDto? CreateAssociationBetweenObjects(RegistryObjectDto? sourceRegistryObject, RegistryObjectDto? targetRegistryObject, string associationType = Constants.Xds.AssociationType.HasMember)
    {
        if (sourceRegistryObject?.Id == null || targetRegistryObject?.Id == null)
            return null;

        var association = new AssociationDto();

        if (targetRegistryObject != null && sourceRegistryObject != null)
        {
            association.AssociationType = associationType;
            association.SourceObject = sourceRegistryObject.Id;
            association.TargetObject = targetRegistryObject.Id;
            association.SubmissionSetStatus = "Current";
        }

        return association;
    }

    public List<SourcePatientInfo>? GetPatientIdentifiersInRegistry()
    {
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();
        var patientIdentifiers = documentRegistry.OfType<DocumentEntryDto>()
            .Select(de => de.SourcePatientInfo)
            .Where(spi => spi != null && !string.IsNullOrWhiteSpace(spi?.PatientId?.Id))
            .DistinctBy(gob => gob?.PatientId?.Id)
            .OfType<SourcePatientInfo>()
            .ToList();

        return patientIdentifiers;
    }

    public RestfulApiResponse DeleteAllDataForPatient(string patientIdentifier)
    {
        var response = new RestfulApiResponse();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntries = documentRegistry.OfType<DocumentEntryDto>().Where(de => de.SourcePatientInfo?.PatientId?.Id == patientIdentifier).ToList();

        if (documentEntries.Count == 0)
        {
            response.SetMessage($"No Metadata found for patient {patientIdentifier}");
            response.Success = false;
            return response;
        }

        foreach (var documentEntry in documentEntries)
        {
            DeleteDocumentAndMetadata(documentEntry.Id);
        }

        response.SetMessage($"Deleted {documentEntries.Count()} entries for patient {patientIdentifier}");

        return response;
    }

    public RestfulApiResponse DeleteUntilTimeSpan(DateTime timeSpan)
    {
        var content = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var oldContent = content.OfType<DocumentEntryDto>().Where(de => de.ServiceStopTime >= timeSpan).ToArray();

        throw new NotImplementedException();
    }

    public RestfulApiResponse DeleteOlderThan(TimeUnit unit, int? days)
    {
        var response = new RestfulApiResponse();

        if (days.HasValue == false)
        {
            response.SetMessage("Specify days to delete");
            return response;
        }

        var registry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var dateInstant = DateTime.Now.AddDays(-days.Value);

        var oldDocumentEntries = registry.OfType<DocumentEntryDto>().Where(de => de.ServiceStopTime < dateInstant).ToArray();

        foreach (var documentEntry in oldDocumentEntries)
        {
            DeleteDocumentAndMetadata(documentEntry.Id);
        }

        response.SetMessage($"Deleted {oldDocumentEntries.Length} entries");
        return response;
    }

    public RestfulApiResponse DeleteByParameters(string[]? patientIdentifier, string[]? securityLabel)
    {
        if (securityLabel == null || securityLabel.Length == 0)
        {
            return new RestfulApiResponse().AddError("MissingParameter", $"{nameof(DeleteByParameters)}: securityLabel cannot be an empty array");
        }

        if (patientIdentifier == null || patientIdentifier.Length == 0)
        {
            return new RestfulApiResponse().AddError("MissingParameter", $"{nameof(DeleteByParameters)}: patientIdentifier cannot be an empty array");
        }

        // Find entries matching criteria
        var patientIdToken = patientIdentifier.Select(cv => cv.Split("|")).Select(pid => new PatientId()
        {
            Id = pid.LastOrDefault(),
            System = pid.FirstOrDefault()?.NoUrn()
        }).ToArray();


        var confidentialityCodes = securityLabel.SelectMany(sp => sp.Split(";")
            .Select(sl =>
            {
                var parts = sl.Split("|");
                return new CodedValue(parts.LastOrDefault(), parts.FirstOrDefault());
            })).ToArray();


        var registry = _registryWrapper.GetDocumentRegistryContentAsDtos().ToArray();

        var matchingDocuments = registry
            .OfType<DocumentEntryDto>()
            .Where(doc => doc.SourcePatientInfo?.PatientId is { } pid &&
                patientIdToken.Any(patid => pid.Id == patid.Id && pid.System == patid.System))
            .ToArray();

        /*
         
        Get set of matching documents where at least one confidentiality code in the document entry's confidentiality codes matches at least one confidentiality code in the request.

        Code kept as comment in case we want to switch to this logic later. 

        matchingDocuments = matchingDocuments
            .Where(doc => doc.ConfidentialityCode != null && doc.ConfidentialityCode
                .Any(docCc => confidentialityCodes
                    .Any(cc => cc.Code == docCc.Code && cc.CodeSystem == docCc.CodeSystem)))
            .ToArray();

        */

        // Get set of matching documents where all confidentiality codes in the request are present in the document entry's confidentiality codes.        
        matchingDocuments = matchingDocuments
            .Where(doc => doc.ConfidentialityCode != null &&
                confidentialityCodes.All(required =>
                    doc.ConfidentialityCode.Any(docCc =>
                        docCc.Code == required.Code &&
                        docCc.CodeSystem == required.CodeSystem)))
            .ToArray();


        var deleteResponses = new List<RestfulApiResponse>();

        foreach (var documentEntry in matchingDocuments)
        {
            deleteResponses.Add(DeleteDocumentAndMetadata(documentEntry.Id));
        }

        var deleteResponseFinal = new RestfulApiResponse()
        {
            Success = deleteResponses.All(dr => dr.Success),
            Message = $"Deleted {deleteResponses.Count(dr => dr.Success)} entries out of {deleteResponses.Count} matching entries"
        };

        return deleteResponseFinal;
    }
}
