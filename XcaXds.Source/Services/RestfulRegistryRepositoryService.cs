using System;
using System.Reflection;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
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

    public DocumentListResponse GetDocumentListForPatient(string? patientId, string? status)
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
            .ByDocumentEntryStatus(status);

        documentListResponse.DocumentListEntries = patientDocumentReferences
            .Select(dr => new DocumentListEntry()
            {
                DocumentReference = dr,
                LinkToDocument = new()
                {
                    Title = dr.Title,
                    Url = $"api/rest/document?home={dr.HomeCommunityId}&repository={dr.RepositoryUniqueId}&document={dr.UniqueId}"
                }
            }).ToList();

        return documentListResponse;
    }

    public DocumentResponse GetDocument(string? home, string? repository, string? document)
    {
        var documentResponse = new DocumentResponse();


        if (string.IsNullOrWhiteSpace(home) || string.IsNullOrWhiteSpace(repository) || string.IsNullOrWhiteSpace(document))
        {
            documentResponse.AddError("MissingParameter", "Parameters homecommunity_id, repository_id, document_id are required.");
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

        if (documentReference.Association == null)
        {
            documentReference.Association = CreateAssociationBetweenObjects(documentReference.DocumentEntry, documentReference.SubmissionSet);
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

    public UpdateResponse UpdateDocumentMetadata(DocumentReferenceDto documentReference)
    {
        var updateResponse = new UpdateResponse();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntryToBeReplaced = documentRegistry
            .OfType<DocumentEntryDto>()
            .Where(ss => ss.Id == documentReference.DocumentEntry?.Id).FirstOrDefault();

        documentRegistry = documentRegistry.Replace(documentEntryToBeReplaced, documentReference.DocumentEntry).ToList();

        var submissionSetToBeReplaced = documentRegistry
            .OfType<SubmissionSetDto>()
            .Where(ss => ss.Id == documentReference.SubmissionSet?.Id).FirstOrDefault();
        documentRegistry = documentRegistry.Replace(submissionSetToBeReplaced, documentReference.SubmissionSet).ToList();

        _registryWrapper.SetDocumentRegistryContentWithDtos(documentRegistry);

        if (documentReference.Document != null && documentReference.Document.Data.Length != 0)
        {
            var storeResult = _repositoryWrapper.StoreDocument(documentReference.Document.DocumentId, documentReference.Document.Data, documentReference.DocumentEntry.PatientId.Code);
            if (storeResult == false)
            {
                updateResponse.AddError("UploadError", "Error while uploading document");
            }
        }

        return updateResponse;
    }

    public RestfulApiResponse PartiallyUpdateDocumentMetadata(DocumentReferenceDto value)
    {
        var partialUpdateResponse = new RestfulApiResponse();
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var submissionSetToPatch = documentRegistry.OfType<SubmissionSetDto>().FirstOrDefault(ro => ro.Id == value.SubmissionSet?.Id);
        var documentEntryToPatch = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.Id == value.DocumentEntry?.Id);
        var associationToPatch = documentRegistry.OfType<AssociationDto>().FirstOrDefault(ro => ro.Id == value.Association?.Id);


        MergeObjects(submissionSetToPatch, value.SubmissionSet);
        MergeObjects(documentEntryToPatch, value.DocumentEntry);
        MergeObjects(associationToPatch, value.Association);



        return partialUpdateResponse;
    }

    public RestfulApiResponse DeleteDocumentAndMetadata(string id)
    {
        var apiResponse = new RestfulApiResponse();
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();
        
        var count = documentRegistry.RemoveAll(x => x.Id == id);

        _registryWrapper.SetDocumentRegistryContentWithDtos(documentRegistry);
        
        apiResponse.SetMessage($"Successfully removed {count} documents");

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

    private AssociationDto CreateAssociationBetweenObjects(DocumentEntryDto documentEntry, SubmissionSetDto submissionSet)
    {
        var association = new AssociationDto();

        if (documentEntry != null && submissionSet != null)
        {
            association.AssociationType = Constants.Xds.AssociationType.HasMember;
            association.SourceObject = submissionSet.Id;
            association.TargetObject = documentEntry.Id;
            association.SubmissionSetStatus = "Current";
        }

        return association;
    }

    public void MergeObjects<T>(T source, T target)
    {
        if (source == null || target == null) return;

        var props = source.GetType().GetProperties();

        foreach (var property in props)
        {
            var sourceValue = property.GetValue(source);
            var targetValue = property.GetValue(target);

            if (sourceValue == null)
                continue;

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                if (targetValue == null)
                {
                    targetValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(target, targetValue);
                }

                MergeObjects(sourceValue, targetValue);
            }
            else
            {
                property.SetValue(source, targetValue);
            }
        }
    }


}
