using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Xca;
using XcaXds.Source.Source;
using static XcaXds.Commons.Constants.Xds.Uuids;

namespace XcaXds.Source.Services;

public class RestfulRegistryRepositoryService
{
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<XdsRegistryService> _logger;
    private readonly RepositoryWrapper _repositoryWrapper;

    public RestfulRegistryRepositoryService(ApplicationConfig appConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper, ILogger<XdsRegistryService> logger, RepositoryWrapper repositoryWrapper)
    {
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public DocumentListResponse GetDocumentListForPatient(string? patientId, string? status, DateTime serviceStartTime, DateTime serviceStopTime, int pageNumber = 1, int pageSize = 10)
    {
        var documentListResponse = new DocumentListResponse();

        if (string.IsNullOrWhiteSpace(patientId))
        {
            documentListResponse.AddError("MissingParameter", "Parameter 'id' is required.");
            return documentListResponse;
        }

        var allowedStatuses = new[] { "Approved", "Deprecated" };

        if (!string.IsNullOrWhiteSpace(status) && !allowedStatuses.Contains(status))
        {
            documentListResponse.AddError("MissingParameter", @$"Status must be one of ""approved"" or ""deprecated"", got {status}.");
            return documentListResponse;
        }

        var patientIdCx = Hl7Object.Parse<CX>(patientId);

        // Account for searches only including the patient Id and not assigning authority (eg api/GetDocumentList?id=13116900216)
        // Add default assigning authority if missing
        patientIdCx.AssigningAuthority ??= new() { UniversalId = Constants.Oid.Fnr, UniversalIdType = Constants.Hl7.UniversalIdType.Iso };

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var patientDocumentReferences = documentRegistry
            .OfType<DocumentEntryDto>()
            .ByDocumentEntryPatientId(patientIdCx)
            .ByDocumentEntryStatus(status)
            .ByDocumentEntryServiceStartTime(serviceStartTime)
            .ByDocumentEntryServiceStopTime(serviceStopTime)
            .ToList();

        var paginatedDocumentList = patientDocumentReferences
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var documentListEntriesWithLink = paginatedDocumentList
            .Select(dr => new DocumentListEntry()
            {
                DocumentReference = dr,
                LinkToDocument = new()
                {
                    Title = dr.Title,
                    Url = $"api/rest/document?home={dr.HomeCommunityId}&repository={dr.RepositoryUniqueId}&document={dr.Id}"
                }
            }).ToList();

        var pagination = new Pagination()
        {
            TotalResults = patientDocumentReferences.Count,
            NumberOfResults = paginatedDocumentList.Count,
            PageNumber = pageNumber,
            Next = paginatedDocumentList.Count < pageSize ? null : pageNumber + 1,
            Prev = pageNumber <= 1 ? null : pageNumber - 1
        };

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

        var documentBytes = _repositoryWrapper.GetDocumentFromRepository(home, repository, document);

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

    public RestfulApiResponse UploadDocumentAndMetadata(DocumentReferenceDto documentReference)
    {
        var uploadResponse = new RestfulApiResponse();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        if (documentReference.Association == null)
        {
            documentReference.Association = CreateAssociationBetweenObjects(documentReference.SubmissionSet, documentReference.DocumentEntry);
        }

        var elementsToBeUploaded = new List<RegistryObjectDto>() { documentReference.DocumentEntry, documentReference.SubmissionSet, documentReference.Association };

        if (DuplicateUuidsExist(documentRegistry, elementsToBeUploaded, out var dupliacteIds))
        {
            uploadResponse.AddError("UniqueIdError", $"Duplicate identifiers found in submission and registry: {string.Join(", ", dupliacteIds)}");
            return uploadResponse;
        }

        if (documentReference.Document != null)
        {
            _repositoryWrapper.StoreDocument(documentReference.Document.DocumentId, documentReference.Document.Data, documentReference.DocumentEntry.PatientId.Code);

        }

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

            _registryWrapper.SetDocumentRegistryContentWithDtos(documentRegistry);

            if (inputDocumentReference.Document != null && inputDocumentReference.Document.Data.Length != 0 && inputDocumentReference.DocumentEntry?.PatientId?.Code != null)
            {
                var storeResult = _repositoryWrapper.StoreDocument(inputDocumentReference.Document.DocumentId, inputDocumentReference.Document.Data, inputDocumentReference.DocumentEntry.PatientId.Code);
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
            _logger.LogInformation($"REPLACE: \nDocumentEntry new ID: {documentEntryId} \nPrevious: {inputDocumentReference.DocumentEntry.Id}");
            inputDocumentReference.DocumentEntry.Id = documentEntryId;


            var submissionSetId = Guid.NewGuid().ToString();
            _logger.LogInformation($"REPLACE: \nSubmissionSet new ID: {submissionSetId} \nPrevious: {inputDocumentReference.SubmissionSet.Id}");
            inputDocumentReference.SubmissionSet.Id = submissionSetId;

            // Deprecate the old DocumentEntry
            documentRegistry = documentRegistry.DeprecateEntry(documentEntryToBeReplaced.Id);

            // Create RPLC association between old and new DocumentEntry
            var replaceAssociation = CreateAssociationBetweenObjects(
                inputDocumentReference.DocumentEntry, 
                documentEntryToBeReplaced,
                Constants.Xds.AssociationType.Replace);

            _logger.LogInformation($"REPLACE: \nReplace Association: {replaceAssociation.Id} Created between {inputDocumentReference.DocumentEntry.Id} and {documentEntryToBeReplaced.Id}");

            // Recreate association with new identifiers
            inputDocumentReference.Association = CreateAssociationBetweenObjects(
                inputDocumentReference.SubmissionSet,
                inputDocumentReference.DocumentEntry);

            _registryWrapper.UpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto>()
            {
                inputDocumentReference.DocumentEntry, 
                inputDocumentReference.SubmissionSet, 
                inputDocumentReference.Association, 
                replaceAssociation
            });

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


        if (documentEntryToPatch != null)
        {
            _logger.LogInformation($"Updating documentEntry {documentEntryToPatch.Id} with values:\n {JsonSerializer.Serialize(value.DocumentEntry, new JsonSerializerOptions() { WriteIndented = true })}");
            MergeObjects(documentEntryToPatch, value.DocumentEntry);
        }

        if (submissionSetToPatch != null)
        {
            _logger.LogInformation($"Updating submissionSet {submissionSetToPatch.Id} with values:\n {JsonSerializer.Serialize(value.SubmissionSet, new JsonSerializerOptions() { WriteIndented = true })}");
            MergeObjects(submissionSetToPatch, value.SubmissionSet);
        }

        if (associationToPatch != null)
        {
            _logger.LogInformation($"Updating association {associationToPatch.Id} with values:\n {JsonSerializer.Serialize(value.Association, new JsonSerializerOptions() { WriteIndented = true })}");
            MergeObjects(associationToPatch, value.Association);
        }

        _registryWrapper.SetDocumentRegistryContentWithDtos(documentRegistry);

        return partialUpdateResponse;
    }

    public RestfulApiResponse DeleteDocumentAndMetadata(string id)
    {
        var apiResponse = new RestfulApiResponse();
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();
        
        var deleteResponse = _repositoryWrapper.DeleteSingleDocument(id);

        if (deleteResponse == false)
        {
            _logger.LogWarning($"Error while deleting document");
            apiResponse.AddError("DeleteError", $"Error while deleting document {id}");
            return apiResponse;
        }

        var associationsForEntry = documentRegistry.OfType<AssociationDto>().Where(assoc => assoc.TargetObject == id).ToList();

        if (associationsForEntry == null)
        {
            apiResponse.SetMessage($"No document with id {id} found");
            return apiResponse;
        }

        var docentryCount = 0;
        var submissionSetCount = 0;
        var associationCount = 0;

        foreach (var association in associationsForEntry)
        {
            var documentEntry = documentRegistry.OfType<RegistryObjectDto>().FirstOrDefault(ss => ss.Id == association?.TargetObject);
            var submissionSet = documentRegistry.OfType<RegistryObjectDto>().FirstOrDefault(ss => ss.Id == association?.SourceObject);

            docentryCount += documentRegistry.RemoveAll(x => x.Id == documentEntry?.Id);
            submissionSetCount += documentRegistry.RemoveAll(x => x.Id == submissionSet?.Id);
            associationCount += documentRegistry.RemoveAll(x => x.Id == association.Id);
        }

        var deleteUpdate = _registryWrapper.SetDocumentRegistryContentWithDtos(documentRegistry);

        if (deleteUpdate == false)
        {
            _logger.LogWarning($"Error while updating registry");
            apiResponse.AddError("RegistryUpdateError", $"Error while deleting document {id}");
            return apiResponse;
        }

        apiResponse.SetMessage($"Successfully removed {docentryCount} DocumentEntries, {submissionSetCount} SubmissisonSets and {associationCount} Associations");

        return apiResponse;
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

    private AssociationDto CreateAssociationBetweenObjects(RegistryObjectDto sourceRegistryObject, RegistryObjectDto targetRegistryObject, string associationType = null)
    {
        var association = new AssociationDto();

        if (targetRegistryObject != null && sourceRegistryObject != null)
        {
            association.AssociationType = associationType ?? Constants.Xds.AssociationType.HasMember;
            association.SourceObject = sourceRegistryObject.Id;
            association.TargetObject = targetRegistryObject.Id;
            association.SubmissionSetStatus = "Current";
        }

        return association;
    }

    public void MergeObjects<T>(T source, T target)
    {
        if (target == null) return;

        var props = target.GetType().GetProperties();

        foreach (var property in props)
        {
            var sourceValue = property.GetValue(source);
            var targetValue = property.GetValue(target);

            if (sourceValue == targetValue)
                continue;

            if (targetValue == null || (sourceValue == null && targetValue == null) || (sourceValue == null && targetValue == null))
                continue;


            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                if (sourceValue == null)
                {
                    sourceValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(source, sourceValue);
                }

                MergeObjects(sourceValue, targetValue);
            }
            else
            {
                if (targetValue != null)
                {
                    property.SetValue(source, targetValue);
                }
            }
        }
    }


}
