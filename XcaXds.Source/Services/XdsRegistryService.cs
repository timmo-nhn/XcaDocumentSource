using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using XcaXds.Commons;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public partial class XdsRegistryService
{
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<XdsRegistryService> _logger;


    public XdsRegistryService(ApplicationConfig xdsConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper, ILogger<XdsRegistryService> logger)
    {
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _logger = logger;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> AppendToRegistryAsync(SoapEnvelope envelope)
    {
        var registryResponse = new RegistryResponseType();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsRegistryObjects();

        var submissionRegistryObjects = envelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList?.ToList();
        if (submissionRegistryObjects == null || submissionRegistryObjects.Count == 0)
        {
            _logger.LogError($"Empty or invalid Registry objects in RegistryObjectList");
            registryResponse.AddError(XdsErrorCodes.XDSRegistryError, $"Empty or invalid Registry objects in RegistryObjectList", "XDS Registry");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);

        }

        if (DuplicateUuidsExist(documentRegistry.RegistryObjectList, submissionRegistryObjects, out string[] duplicateIds))
        {
            _logger.LogError($"Duplicate UUIDs in request and/or registry {string.Join(", ", duplicateIds)}");
            registryResponse.AddError(XdsErrorCodes.XDSDuplicateUniqueIdInRegistry, $"Duplicate UUIDs in request and/or registry {string.Join(", ", duplicateIds)}", "XDS Registry");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        // RPLC option, replacing and deprecating document entries
        var documentReplaceAssociations = submissionRegistryObjects.OfType<AssociationType>()
            .Where(assoc => assoc.AssociationTypeData == Constants.Xds.AssociationType.Replace);

        foreach (var replaceAssociation in documentReplaceAssociations)
        {
            var documentId = replaceAssociation.TargetObject;
            documentRegistry.RegistryObjectList.DeprecateDocumentEntry(documentId, out bool success);

            if (success)
            {
                _logger.LogInformation($"Deprecating document with id {documentId}");
            }
        }


        // list spread to combine both lists
        documentRegistry.RegistryObjectList = [.. documentRegistry.RegistryObjectList, .. submissionRegistryObjects];

        var registryUpdateResult = _registryWrapper.SetDocumentRegistryFromXml(documentRegistry);

        if (registryUpdateResult.IsSuccess)
        {
            registryResponse.EvaluateStatusCode();
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Error while updating registry");
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public async Task<SoapRequestResult<SoapEnvelope>> RegistryStoredQueryAsync(SoapEnvelope soapEnvelope)
    {
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsRegistryObjects();

        var registryResponse = new RegistryResponseType();

        var adhocQueryRequest = soapEnvelope.Body.AdhocQueryRequest;

        var filteredElements = new List<IdentifiableType>();

        switch (adhocQueryRequest?.AdhocQuery.Id)
        {
            case Constants.Xds.StoredQueries.FindDocuments:
                var findDocumentsSearchParameters = RegistryStoredQueryParameters.GetFindDocumentsParameters(adhocQueryRequest.AdhocQuery);
                var registryFindDocumentEntriesResult = documentRegistry.RegistryObjectList
                    .OfType<ExtrinsicObjectType>();

                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryPatientId(findDocumentsSearchParameters.XdsDocumentEntryPatientId);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryClassCode(findDocumentsSearchParameters.XdsDocumentEntryClassCode);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryTypeCode(findDocumentsSearchParameters.XdsDocumentEntryTypeCode);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryPracticeSettingCode(findDocumentsSearchParameters.XdsDocumentEntryPracticeSettingCode);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryCreationTimeFrom(findDocumentsSearchParameters.XdsDocumentEntryCreationTimeFrom);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryCreationTimeTo(findDocumentsSearchParameters.XdsDocumentEntryCreationTimeTo);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryServiceStartTimeFrom(findDocumentsSearchParameters.XdsDocumentEntryServiceStartTimeFrom);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryServiceStartTimeTo(findDocumentsSearchParameters.XdsDocumentEntryServiceStartTimeTo);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryServiceStopTimeFrom(findDocumentsSearchParameters.XdsDocumentEntryServiceStoptimeFrom);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryServiceStopTimeTo(findDocumentsSearchParameters.XdsDocumentEntryServiceStoptimeTo);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryHealthcareFacilityTypeCode(findDocumentsSearchParameters.XdsDocumentEntryHealthcareFacilityTypeCode);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryEventCodeList(findDocumentsSearchParameters.XdsDocumentEntryEventCodeList);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryConfidentialityCode(findDocumentsSearchParameters.XdsDocumentEntryConfidentialityCode);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryAuthorPerson(findDocumentsSearchParameters.XdsDocumentEntryAuthorPerson);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentFormatCode(findDocumentsSearchParameters.XdsDocumentEntryFormatCode);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryStatus(findDocumentsSearchParameters.XdsDocumentEntryStatus);
                registryFindDocumentEntriesResult = registryFindDocumentEntriesResult
                    .ByDocumentEntryType(findDocumentsSearchParameters.XdsDocumentEntryType)
                    .ToList();


                if (findDocumentsSearchParameters.XdsDocumentEntryPatientId == null)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing or malformed required parameter $XDSDocumentEntryPatientId {findDocumentsSearchParameters.XdsDocumentEntryPatientId}".Trim(), "XDS Registry");
                }
                if (findDocumentsSearchParameters.XdsDocumentEntryStatus == null || findDocumentsSearchParameters.XdsDocumentEntryStatus.Count == 0)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $XDSDocumentEntryStatus {string.Join(" ", findDocumentsSearchParameters.XdsDocumentEntryStatus ?? new List<string[]>())}".Trim(), "XDS Registry");
                }
                filteredElements = [.. registryFindDocumentEntriesResult];


                break;

            case Constants.Xds.StoredQueries.FindSubmissionSets:
                //registryFindSubmissionSetsResult = FilterRegistryOnSubmissionSets(adhocQueryRequest.AdhocQuery);
                var findSubmissionSetsParameters = RegistryStoredQueryParameters.GetFindSubmissionSetsParameters(adhocQueryRequest.AdhocQuery);

                var registryFindSubmissionSetsResult = documentRegistry.RegistryObjectList
                    .OfType<RegistryPackageType>();

                registryFindSubmissionSetsResult = registryFindSubmissionSetsResult
                    .BySubmissionSetPatientId(findSubmissionSetsParameters.XdsSubmissionSetPatientId);
                registryFindSubmissionSetsResult = registryFindSubmissionSetsResult
                    .BySubmissionSetSourceId(findSubmissionSetsParameters.XdsSubmissionSetSourceId);
                registryFindSubmissionSetsResult = registryFindSubmissionSetsResult
                    .BySubmissionSetSubmissionTimeFrom(findSubmissionSetsParameters.XdsSubmissionSetSubmissionTimeFrom);
                registryFindSubmissionSetsResult = registryFindSubmissionSetsResult
                    .BySubmissionSetSubmissionTimeTo(findSubmissionSetsParameters.XdsSubmissionSetSubmissionTimeTo);

                if (findSubmissionSetsParameters.XdsSubmissionSetPatientId == null)
                {
                    _logger.LogError($"Missing or malformed required parameter $XDSDocumentEntryPatientId");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing or malformed required parameter $XdsSubmissionSetPatientId {findSubmissionSetsParameters.XdsSubmissionSetPatientId}".Trim(), "XDS Registry");
                }
                if (findSubmissionSetsParameters.XdsSubmissionSetStatus == null || findSubmissionSetsParameters.XdsSubmissionSetStatus.Count == 0)
                {
                    _logger.LogError($"Missing required parameter $XDSDocumessntEntryStatus");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $XdsSubmissionSetStatus {string.Join(" ", findSubmissionSetsParameters.XdsSubmissionSetStatus ?? new List<string[]>())}".Trim(), "XDS Registry");
                }

                filteredElements = [.. registryFindSubmissionSetsResult];

                break;

            case Constants.Xds.StoredQueries.FindFolders:
                var findFoldersParameters = RegistryStoredQueryParameters.GetFindFoldersParameters(adhocQueryRequest.AdhocQuery);

                var registryFindFoldersResult = documentRegistry.RegistryObjectList
                    .OfType<RegistryPackageType>();

                registryFindFoldersResult = registryFindFoldersResult
                    .ByXdsFolderPatientId(findFoldersParameters.XdsFolderPatientId);

                if (findFoldersParameters.XdsFolderPatientId == null)
                {
                    _logger.LogError($"Missing required parameter $XDSDocumessntEntryStatus");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing or malformed required parameter $XdsFolderPatientId {findFoldersParameters.XdsFolderPatientId}".Trim(), "XDS Registry");
                }
                if (findFoldersParameters.XdsFolderStatus == null || findFoldersParameters.XdsFolderStatus.Count == 0)
                {
                    _logger.LogError($"Missing required parameter $XDSDocumessntEntryStatus");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $XdsFolderStatus {string.Join(" ", findFoldersParameters.XdsFolderStatus ?? new List<string[]>())}".Trim(), "XDS Registry");
                }

                filteredElements = [.. registryFindFoldersResult];

                break;

            case Constants.Xds.StoredQueries.GetAssociations:
                var getAssociationsParameters = RegistryStoredQueryParameters.GetAssociationsParameters(adhocQueryRequest.AdhocQuery);

                var registryGetAssociationsResult = documentRegistry.RegistryObjectList
                    .OfType<AssociationType>();

                registryGetAssociationsResult = registryGetAssociationsResult
                    .ByUuid(getAssociationsParameters.Uuid);
                registryGetAssociationsResult = registryGetAssociationsResult
                    .ByHomeCommunityId(adhocQueryRequest.AdhocQuery.Home);

                if (getAssociationsParameters.Uuid == null || getAssociationsParameters.Uuid.Count == 0)
                {
                    _logger.LogError($"Missing required parameter $uuid was not set");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $uuid not set".Trim(), "XDS Registry");
                }

                filteredElements = [.. registryGetAssociationsResult];

                break;

            case Constants.Xds.StoredQueries.GetFolders:
                var getFoldersParameters = RegistryStoredQueryParameters.GetFoldersParameters(adhocQueryRequest.AdhocQuery);

                var registryGetFoldersResult = documentRegistry.RegistryObjectList
                    .OfType<RegistryPackageType>();

                registryGetFoldersResult = registryGetFoldersResult
                    .ByXdsFolderUniqueId(getFoldersParameters.XdsFolderUniqueId);

                registryGetFoldersResult = registryGetFoldersResult
                    .ByXdsFolderEntryUuid(getFoldersParameters.XdsFolderEntryUuid);

                // https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7.6
                // Return an XDSStoredQueryParamNumber error if both parameters are specified
                if (getFoldersParameters.XdsFolderUniqueId?.Count != 0 && getFoldersParameters.XdsFolderEntryUuid?.Count != 0)
                {
                    _logger.LogError($"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryParamNumber, $"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified".Trim(), "XDS Registry");
                }

                filteredElements = [.. registryGetFoldersResult];

                break;


            //case Constants.Xds.StoredQueries.GetFolderAndContents:
            //    var getFoldersAndContentsParameters = RegistryStoredQueryParameters.GetFolderAndContentsParameters(adhocQueryRequest.AdhocQuery);

            //    var registryGetFoldersAndDocumentsResult = documentRegistry.RegistryObjectList.OfType<IdentifiableType>();

            //    registryGetFoldersAndDocumentsResult = registryGetFoldersAndDocumentsResult
            //        .ByXdsFolderUniqueId(getFoldersAndContentsParameters.XdsFolderUniqueId);

            //    registryGetFoldersAndDocumentsResult = registryGetFoldersAndDocumentsResult
            //        .ByXdsFolderEntryUuid(getFoldersAndContentsParameters.XdsFolderEntryUuid);

            //    registryGetFoldersAndDocumentsResult = registryGetFoldersAndDocumentsResult
            //        .ByXdsDocumentEntryFormatCode(getFoldersAndContentsParameters.XdsDocumentEntryFormatCode);


            //    // https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7.11
            //    // Return an XDSStoredQueryParamNumber error if both parameters are specified
            //    if (getFoldersAndContentsParameters.XdsFolderUniqueId != null && getFoldersAndContentsParameters.XdsFolderEntryUuid != null)
            //    {
            //        _logger.LogError($"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified");
            //        registryResponse.AddError(XdsErrorCodes.XDSStoredQueryParamNumber, $"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified".Trim(), "XDS Registry");
            //    }

            //    filteredElements = [.. registryGetFoldersAndDocumentsResult];

            //    break;
        }

        if (adhocQueryRequest.ResponseOption != null)
        {
            switch (adhocQueryRequest.ResponseOption.ReturnType)
            {
                case ResponseOptionTypeReturnType.ObjectRef:
                    // Only return the unique identifiers and HomeCommunityId
                    var objectRefs = filteredElements
                        .Select(eo => new ObjectRefType() { Id = eo.Id }).ToList();
                    filteredElements = [.. objectRefs];
                    break;
                case ResponseOptionTypeReturnType.LeafClass:
                    break;

                default:
                    break;
            }
        }
        else
        {
            _logger.LogError($"ResponseOption was not specified, must be either LeafClass or ObjectRef");
            registryResponse.AddError(XdsErrorCodes.XDSStoredQueryParamNumber, $"ResponseOption was not specified".Trim(), "XDS Registry");
        }


        registryResponse.EvaluateStatusCode();

        var responseEnvelope = new SoapEnvelope()
        {
            Header = new()
            {
                RelatesTo = soapEnvelope.Header?.MessageId
            },
            Body = new()
            {
                AdhocQueryResponse = new()
                {
                    RegistryObjectList = [.. filteredElements],
                    RegistryErrorList = registryResponse.RegistryErrorList,
                    Status = registryResponse.Status
                }
            }
        };
        _logger.LogInformation($"Registry Stored Query Complete, returned {filteredElements.Count} XDSEntries");
        return new SoapRequestResult<SoapEnvelope>() { Value = responseEnvelope, IsSuccess = true };
    }

    public async Task<SoapRequestResult<SoapEnvelope>> DeleteDocumentSetAsync(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();
        var removeObjectsRequest = soapEnvelope.Body.RemoveObjectsRequest;
        var registryContent = _registryWrapper.GetDocumentRegistryContentAsRegistryObjects();

        var objectRefList = removeObjectsRequest.ObjectRefList.ObjectRef;


        var removedDocumentsCount = registryContent.RegistryObjectList.RemoveAll(registryObject =>
            objectRefList.Any(or => or.Id == registryObject.Id));

        // Skip if nothing was removed
        if (removedDocumentsCount == 0)
        {
            registryResponse.EvaluateStatusCode();
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var registryUpdateResult = _registryWrapper.UpdateDocumentRegistryFromXml(registryContent);
        registryResponse.EvaluateStatusCode();
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> CopyIti41ToIti42Message(SoapEnvelope iti41Message)
    {
        var registryResponse = new RegistryResponseType();

        var iti42Message = new SoapEnvelope()
        {
            Header = iti41Message.Header,
            Body = new() { RegisterDocumentSetRequest = new() { SubmitObjectsRequest = new() } }
        };

        iti42Message.Body.RegisterDocumentSetRequest.SubmitObjectsRequest = iti41Message.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest;
        iti42Message.SetAction(Constants.Xds.OperationContract.Iti42Action);


        var registryObjectList = iti42Message.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;

        var associations = registryObjectList.OfType<AssociationType>().ToArray();
        var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var documentEntries = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var documents = iti41Message.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        foreach (var association in associations)
        {
            if (association.AssociationTypeData is not Constants.Xds.AssociationType.HasMember) continue;

            var assocDocument = documents?.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocRegistryPackage = registryPackages.FirstOrDefault(eo => eo.Id.NoUrn() == association.SourceObject.NoUrn());

            if (assocExtrinsicObject is null && assocDocument is not null && assocRegistryPackage is null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingDocumentMetadata, $"Missing document metadata for document with ID {assocDocument?.Id}", "XDS Registry");
                continue;
            }
            if (assocExtrinsicObject is null || assocDocument is null || assocRegistryPackage is null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingDocument, $"Missing document/sourceobject/targetobject for association {association.Id}", "XDS Registry");
                continue;
            }

            // Home attribute on extrinsicobject
            if (string.IsNullOrWhiteSpace(assocExtrinsicObject.Home))
            {
                assocExtrinsicObject.Home = _xdsConfig.HomeCommunityId;
                assocRegistryPackage.Home = _xdsConfig.HomeCommunityId;
            }

            // Document Hash slot
            var documentHash = Convert.ToHexString(SHA1.HashData(assocDocument.Value));

            // Check if the submissionset already has a hash, and if its the same as the calculated hash
            var extrinsicObjectHash = assocExtrinsicObject.GetSlots("Hash").FirstOrDefault()?.GetFirstValue();
            if (!string.IsNullOrWhiteSpace(extrinsicObjectHash) && extrinsicObjectHash != documentHash)
            {
                registryResponse.AddError(XdsErrorCodes.XDSNonIdenticalHash, "Document hash was not equal to hash value in extrinsic object", "XDS Registry");
                return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
            }
            assocExtrinsicObject.AddSlot(Constants.Xds.SlotNames.Hash, [documentHash]);

            // RepositoryUniqueId
            assocExtrinsicObject.AddSlot(Constants.Xds.SlotNames.RepositoryUniqueId, [_xdsConfig.RepositoryUniqueId]);

            // Document Size slot
            var documentSize = assocDocument.Value.Length;
            assocExtrinsicObject.AddSlot(Constants.Xds.SlotNames.Size, [documentSize.ToString()]);

            // Document LegalAuthenticator slot
            var authorPersonSlot = assocExtrinsicObject.Classification
                .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.SubmissionSet.Author)?.Slot
                .FirstOrDefault(sl => sl.Name == Constants.Xds.SlotNames.AuthorPerson);

            if (authorPersonSlot != null)
            {
                assocExtrinsicObject.AddSlot(authorPersonSlot);
            }


            // Switch from SubmissionSet UUIDs to XdsDocumentEntry UUIDs
            foreach (var classification in assocExtrinsicObject.Classification)
            {
                classification.ClassificationScheme = classification.ClassificationScheme switch
                {
                    Constants.Xds.Uuids.SubmissionSet.Author => Constants.Xds.Uuids.DocumentEntry.Author,
                    _ => classification.ClassificationScheme
                };
            }
        }
        iti42Message.Body.RegistryResponse = registryResponse;
        return SoapExtensions.CreateSoapResultResponse(iti42Message);
    }

    public SoapRequestResult<string> UpdateDocumentRegistryFromXml(XmlDocumentRegistry xml)
    {
        try
        {
            var dtoList = RegistryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(xml.RegistryObjectList);

            _registryWrapper.SetDocumentRegistryContentWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }

    private bool DuplicateUuidsExist(List<IdentifiableType> registryObjectList, List<IdentifiableType> submissionRegistryObjects, out string[] duplicateIds)
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
}