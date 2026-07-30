using Hl7.Fhir.Model;
using Hl7.FhirPath.Sprache;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.Shared.Extensions;

namespace XcaXds.WebService.Services.XdsRegistry;

public partial class XdsRegistryService
{
    private readonly ILogger<XdsRegistryService> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly XdsSubmitObjectsValidator _submitObjectsValidator;
    private readonly DocumentObfuscationService _documentObfuscationService;
    private readonly BusinessLogicFiltersRegistry _businessLogicFiltersRegistry;
    private readonly BusinessLogicMapperService _businessLogicMapperService;
    private readonly RegistryMetadataTransformerService _registryMetadataTransformerService;

    private static Dictionary<string, string> AdhocQueries = ConstantsExtensions.GetAsDictionary(typeof(Constants.Xds.StoredQueries));

    public XdsRegistryService(
        ILogger<XdsRegistryService> logger,
        ApplicationConfig xdsConfig,
        RegistryWrapper registryWrapper,
        XdsSubmitObjectsValidator submitObjectsValidator,
        DocumentObfuscationService documentObfuscationService,
        BusinessLogicFiltersRegistry businessLogicFiltersRegistry,
        BusinessLogicMapperService businessLogicMapperService,
        RegistryMetadataTransformerService registryMetadataTransformerService)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _submitObjectsValidator = submitObjectsValidator;
        _documentObfuscationService = documentObfuscationService;
        _businessLogicFiltersRegistry = businessLogicFiltersRegistry;
        _businessLogicMapperService = businessLogicMapperService;
        _registryMetadataTransformerService = registryMetadataTransformerService;
    }

    public static void ValidateRecursive(object? obj, List<ValidationResult> results)
    {
        if (obj == null) return;

        var context = new ValidationContext(obj);
        Validator.TryValidateObject(obj, context, results, true);

        var properties = obj.GetType().GetProperties().Where(prop => prop.PropertyType != typeof(string) && prop.PropertyType != typeof(System.Char));

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);

            if (value == null) continue;

            if (value is IEnumerable<object> collection)
            {
                foreach (var item in collection)
                    ValidateRecursive(item, results);
            }
            else
            {
                ValidateRecursive(value, results);
            }
        }
    }

    public SoapRequestResult<SoapEnvelope> AppendToRegistry(SoapEnvelope? envelope, bool validateOnly = false)
    {
        var registryResponse = new RegistryResponseType();

        if (envelope == null)
        {
            return registryResponse
                .AddError(XdsErrorCodes.XDSRegistryError, "No SOAP envelope provided", "XDS Registry")
                .CreateSoapResult();
        }

        var submissionRegistryObjects = envelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList?.ToArray();
        if (submissionRegistryObjects?.Length.IsNullOrZero() == true)
        {
            return registryResponse
                .AddError(XdsErrorCodes.XDSRegistryError, "Missing SubmitObjectsRequest", "XDS Registry")
                .CreateSoapResult();
        }

        var validationIssues = _submitObjectsValidator.ValidateSubmitObjectsRequest(submissionRegistryObjects);

        var results = new List<ValidationResult>();
        ValidateRecursive(submissionRegistryObjects, results);

        if (results.Count > 0)
        {
            foreach (var e in results)
            {
                _logger.LogError(e.ErrorMessage + " Members: " + string.Join(' ', e.MemberNames));
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Validation Errors: " + e.ErrorMessage + " Members: " + string.Join(' ', e.MemberNames), _xdsConfig.HomeCommunityId);
            }
            return registryResponse.CreateSoapResult();
        }

        _logger.LogInformation($"Validation of SubmitObjectsRequest completed with {validationIssues.Length} issue(s)");

        if (validationIssues.Length > 0)
        {
            foreach (var error in validationIssues)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Validation Errors: " + error.Message, _xdsConfig.HomeCommunityId);
            }
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var invalidMimetypes = submissionRegistryObjects?.OfType<ExtrinsicObjectType>().Where(sro => sro.MimeType.IsAnyOf(_businessLogicFiltersRegistry.GetAllowedMimeTypes()) == false).ToArray();

        if (invalidMimetypes?.Length > 0)
        {
            _logger.LogError($"{envelope?.Header.MessageId} - Invalid MimeType detected in RegistryObjectList");
            foreach (var extObjt in invalidMimetypes)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, $"Invalid MimeType: {extObjt.MimeType} for DocumentEntry with Id: {extObjt.Id}, Document Id: {extObjt.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value}", "XDS Registry");
            }
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        if (submissionRegistryObjects == null || submissionRegistryObjects.Length == 0)
        {
            _logger.LogError($"{envelope?.Header.MessageId} - Empty or invalid Registry objects in RegistryObjectList");

            return registryResponse
                .AddError(XdsErrorCodes.XDSRegistryError, $"Empty or invalid Registry objects in RegistryObjectList", "XDS Registry")
                .CreateSoapResult();
        }

        var registryObjects = _registryWrapper.GetDocumentRegistryContentAsRegistryObjects();
        if (DuplicateUuidsExist(registryObjects, submissionRegistryObjects, out string[] duplicateIds))
        {
            _logger.LogError($"{envelope?.Header.MessageId} - Duplicate UUIDs in request and/or registry {string.Join(", ", duplicateIds)}");

            return registryResponse
                .AddError(XdsErrorCodes.XDSDuplicateUniqueIdInRegistry, $"Duplicate UUIDs in request and/or registry {string.Join(", ", duplicateIds)}", "XDS Registry")
                .CreateSoapResult();
        }

        // RPLC option, replacing and deprecating document entries
        var documentReplaceAssociations = submissionRegistryObjects.OfType<AssociationType>()
            .Where(assoc => assoc.AssociationTypeData == Constants.Xds.AssociationType.Replace).ToList();

        foreach (var replaceAssociation in documentReplaceAssociations)
        {
            var documentId = replaceAssociation.TargetObject;
            if (string.IsNullOrWhiteSpace(documentId)) continue;

            if (registryObjects.TryDeprecateDocumentEntry(documentId, out var deprecatedEntry) && deprecatedEntry != null)
            {
                // We will use Upsert-logic to update the registry later, which will
                // overwrite the existing DbEntities with now deprecated entries from the out-variable.
                // This is the safest option to avoid materialization issues with IEnumerable...
                submissionRegistryObjects = [.. submissionRegistryObjects, deprecatedEntry];
            }
            else
            {
                _logger.LogWarning($"{envelope?.Header.MessageId} - Error deprecating document - id {documentId} not found");
                continue;
            }

            _logger.LogInformation($"{envelope?.Header.MessageId} - Successfully deprecated document with id {documentId}");

            var deprecatedEntriesSoFar = submissionRegistryObjects.OfType<ExtrinsicObjectType>().Where(ro => ro.Status == Constants.Xds.StatusValues.Deprecated).ToArray();
        }

        registryResponse.EvaluateStatusCode();
        if (validateOnly == false)
        {
            var sxmls = new SoapXmlSerializer();
            var gobb = sxmls.SerializeSoapMessageToXmlString(submissionRegistryObjects).Content;
            var submissionElementsToUpdate = _registryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(submissionRegistryObjects).ToList();

            _registryMetadataTransformerService.TransformFhirConceptsToXdsConcepts(submissionElementsToUpdate);

            var response = _registryWrapper.InsertOrUpdateDocumentRegistryContentWithDtos(submissionElementsToUpdate);
            if (response.IsSuccess)
            {
                return registryResponse.CreateSoapResult();
            }
            else
            {
                return registryResponse
                    .AddError(XdsErrorCodes.XDSRepositoryError, $"Error while updating registry\n {response.Message}", _xdsConfig.HomeCommunityId)
                    .CreateSoapResult();
            }
        }

        return registryResponse.CreateSoapResult();
    }

    public SoapRequestResult<SoapEnvelope> RegistryStoredQuery(SoapEnvelope soapEnvelope)
    {
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsRegistryObjects();

        var registryResponse = new RegistryResponseType();
        var enumeratedEntriesResult = new List<IdentifiableType>();

        var adhocQueryRequest = soapEnvelope.Body.AdhocQueryRequest;

        var returnType = adhocQueryRequest?.ResponseOption?.ReturnType;
        var isLeafClass = returnType == ResponseOptionTypeReturnType.LeafClass;
        var isObjectRef = returnType == ResponseOptionTypeReturnType.ObjectRef;


        switch (adhocQueryRequest?.AdhocQuery?.Id)
        {
            case Constants.Xds.StoredQueries.FindDocuments:
                var findDocumentsSearchParameters = RegistryStoredQueryParameters.GetFindDocumentsParameters(adhocQueryRequest.AdhocQuery);

                var patientIdentifier = Hl7Object.Parse<CX>(findDocumentsSearchParameters.XdsDocumentEntryPatientId)
                    is { } patId
                    ? new PatientId(patId.IdNumber, patId.AssigningAuthority?.UniversalId)
                    : null;

                var prefilteredDocumentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtosByPatientId(patientIdentifier);
                RegistryStoredQueryParameters.AddAlternateRepresentationsForCodeSystems(findDocumentsSearchParameters);

                var registryFindDocumentEntriesResult = prefilteredDocumentRegistry?
                    .OfType<DocumentEntryDto>()
                    .Select(RegistryMetadataTransformerService.TransformRegistryObjectDtoToRegistryObjectStateless)
                    .OfType<ExtrinsicObjectType>() ?? [];

                _logger.LogDebug($"{soapEnvelope.Header.MessageId} - FindDocuments parameters:\n" + JsonSerializer.Serialize(findDocumentsSearchParameters, Constants.JsonDefaultOptions.DefaultSettings));

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
                    .ByDocumentEntryType(findDocumentsSearchParameters.XdsDocumentEntryType);


                if (findDocumentsSearchParameters.XdsDocumentEntryPatientId == null)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing or malformed required parameter $XDSDocumentEntryPatientId {findDocumentsSearchParameters.XdsDocumentEntryPatientId}".Trim(), "XDS Registry");
                }
                if (findDocumentsSearchParameters.XdsDocumentEntryStatus == null || findDocumentsSearchParameters.XdsDocumentEntryStatus.Count == 0)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $XDSDocumentEntryStatus {string.Join(" ", findDocumentsSearchParameters.XdsDocumentEntryStatus ?? new List<string[]>())}".Trim(), "XDS Registry");
                }

                IEnumerable<IdentifiableType> registryElements = registryFindDocumentEntriesResult;


                // Materialize the query results before doing more granular filtering
                enumeratedEntriesResult = [.. registryElements ?? []];

                var count = enumeratedEntriesResult.Count;

                // Safe guard to avoid duplicate IDs in response
                enumeratedEntriesResult = enumeratedEntriesResult?
                    .GroupBy(x => x.Id)
                    .Select(g => g.First())
                    .ToList();

                _logger.LogDebug($"{soapEnvelope?.Header.MessageId} - Patient Identifier: {findDocumentsSearchParameters.XdsDocumentEntryPatientId}");

                break;

            case Constants.Xds.StoredQueries.FindSubmissionSets:
                //registryFindSubmissionSetsResult = FilterRegistryOnSubmissionSets(adhocQueryRequest.AdhocQuery);
                var findSubmissionSetsParameters = RegistryStoredQueryParameters.GetFindSubmissionSetsParameters(adhocQueryRequest.AdhocQuery);

                var registryFindSubmissionSetsResult = documentRegistry
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
                    _logger.LogError($"{soapEnvelope?.Header.MessageId} - Missing or malformed required parameter $XDSDocumentEntryPatientId");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing or malformed required parameter $XdsSubmissionSetPatientId {findSubmissionSetsParameters.XdsSubmissionSetPatientId}".Trim(), "XDS Registry");
                }
                if (findSubmissionSetsParameters.XdsSubmissionSetStatus == null || findSubmissionSetsParameters.XdsSubmissionSetStatus.Count == 0)
                {
                    _logger.LogError($"{soapEnvelope?.Header.MessageId} - Missing required parameter $XDSDocumessntEntryStatus");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $XdsSubmissionSetStatus {string.Join(" ", findSubmissionSetsParameters.XdsSubmissionSetStatus ?? new List<string[]>())}".Trim(), "XDS Registry");
                }

                enumeratedEntriesResult = [.. registryFindSubmissionSetsResult];

                break;

            case Constants.Xds.StoredQueries.FindFolders:
                var findFoldersParameters = RegistryStoredQueryParameters.GetFindFoldersParameters(adhocQueryRequest.AdhocQuery);

                var registryFindFoldersResult = documentRegistry
                    .OfType<RegistryPackageType>();

                registryFindFoldersResult = registryFindFoldersResult
                    .ByXdsFolderPatientId(findFoldersParameters.XdsFolderPatientId);

                if (findFoldersParameters.XdsFolderPatientId == null)
                {
                    _logger.LogError($"{soapEnvelope?.Header.MessageId} - Missing required parameter $XDSDocumessntEntryStatus");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing or malformed required parameter $XdsFolderPatientId {findFoldersParameters.XdsFolderPatientId}".Trim(), "XDS Registry");
                }
                if (findFoldersParameters.XdsFolderStatus == null || findFoldersParameters.XdsFolderStatus.Count == 0)
                {
                    _logger.LogError($"{soapEnvelope?.Header.MessageId} - Missing required parameter $XDSDocumessntEntryStatus");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $XdsFolderStatus {string.Join(" ", findFoldersParameters.XdsFolderStatus ?? new List<string[]>())}".Trim(), "XDS Registry");
                }

                enumeratedEntriesResult = [.. registryFindFoldersResult];

                break;

            case Constants.Xds.StoredQueries.GetAssociations:
                var getAssociationsParameters = RegistryStoredQueryParameters.GetAssociationsParameters(adhocQueryRequest.AdhocQuery);

                var registryGetAssociationsResult = documentRegistry
                    .OfType<AssociationType>();

                registryGetAssociationsResult = registryGetAssociationsResult
                    .ByUuid(getAssociationsParameters.Uuid);

                //registryGetAssociationsResult = registryGetAssociationsResult
                //.ByHomeCommunityId(adhocQueryRequest.AdhocQuery.Home);    // Associations do not have Home attribute so it's always null in DTOs

                if (getAssociationsParameters.Uuid == null || getAssociationsParameters.Uuid.Count == 0)
                {
                    _logger.LogError($"{soapEnvelope?.Header.MessageId} - Missing required parameter $uuid was not set");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, $"Missing required parameter $uuid not set".Trim(), "XDS Registry");
                }

                enumeratedEntriesResult = [.. registryGetAssociationsResult];

                break;

            // HAYO! Folders are actually not supported...
            case Constants.Xds.StoredQueries.GetFolders:
                var getFoldersParameters = RegistryStoredQueryParameters.GetFoldersParameters(adhocQueryRequest.AdhocQuery);

                var registryGetFoldersResult = documentRegistry
                    .OfType<RegistryPackageType>();

                registryGetFoldersResult = registryGetFoldersResult
                    .ByXdsFolderUniqueId(getFoldersParameters.XdsFolderUniqueId);

                registryGetFoldersResult = registryGetFoldersResult
                    .ByXdsFolderEntryUuid(getFoldersParameters.XdsFolderEntryUuid);

                // https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7.6
                // Return an XDSStoredQueryParamNumber error if both parameters are specified
                if (getFoldersParameters.XdsFolderUniqueId?.Count > 0 && getFoldersParameters.XdsFolderEntryUuid?.Count > 0)
                {
                    _logger.LogError($"{soapEnvelope?.Header.MessageId} - Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryParamNumber, $"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified".Trim(), "XDS Registry");
                }

                enumeratedEntriesResult = [.. registryGetFoldersResult];

                break;


            case Constants.Xds.StoredQueries.GetFolderAndContents:
                var getFoldersAndContentsParameters = RegistryStoredQueryParameters.GetFolderAndContentsParameters(adhocQueryRequest.AdhocQuery);

                var registryGetFoldersAndDocumentsResult = documentRegistry.OfType<IdentifiableType>();

                registryGetFoldersAndDocumentsResult = registryGetFoldersAndDocumentsResult
                    .ByXdsFolderUniqueId(getFoldersAndContentsParameters.XdsFolderUniqueId);

                registryGetFoldersAndDocumentsResult = registryGetFoldersAndDocumentsResult
                    .ByXdsFolderEntryUuid(getFoldersAndContentsParameters.XdsFolderEntryUuid);

                registryGetFoldersAndDocumentsResult = registryGetFoldersAndDocumentsResult
                    .ByXdsDocumentEntryFormatCode(getFoldersAndContentsParameters.XdsDocumentEntryFormatCode);


                // https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7.11
                // Return an XDSStoredQueryParamNumber error if both parameters are specified
                if (getFoldersAndContentsParameters.XdsFolderUniqueId != null && getFoldersAndContentsParameters.XdsFolderEntryUuid != null)
                {
                    _logger.LogError($"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified");
                    registryResponse.AddError(XdsErrorCodes.XDSStoredQueryParamNumber, $"Either $XDSFolderEntryUUID or $XDSFolderUniqueId shall be specified".Trim(), "XDS Registry");
                }

                enumeratedEntriesResult = [.. registryGetFoldersAndDocumentsResult];

                break;
        }

        if (adhocQueryRequest?.ResponseOption != null)
        {
            switch (adhocQueryRequest.ResponseOption.ReturnType)
            {
                case ResponseOptionTypeReturnType.ObjectRef:
                    // Only return the unique identifiers and HomeCommunityId
                    if (enumeratedEntriesResult?.Count > 0)
                    {
                        var objectRefs = enumeratedEntriesResult
                            .Select(eo => new ObjectRefType() { Id = eo.Id }).ToList();
                        enumeratedEntriesResult = [.. objectRefs];
                    }
                    break;

                case ResponseOptionTypeReturnType.LeafClass:
                    break;

                default:
                    break;
            }
        }
        else
        {
            _logger.LogError($"{soapEnvelope?.Header.MessageId} - ResponseOption was not specified, must be either 'LeafClass' or 'ObjectRef'");
            registryResponse.AddError(XdsErrorCodes.XDSStoredQueryParamNumber, $"ResponseOption was not specified".Trim(), "XDS Registry");
        }

        registryResponse.EvaluateStatusCode();

        var responseEnvelope = new SoapEnvelope()
        {
            Header = new()
            {
                RelatesTo = soapEnvelope?.Header.MessageId
            },
            Body = new()
            {
                AdhocQueryResponse = new()
                {
                    RegistryErrorList = registryResponse.RegistryErrorList,
                    Status = registryResponse.Status
                }
            }
        };


        if (enumeratedEntriesResult?.Count > 0)
        {
            responseEnvelope.Body.AdhocQueryResponse.RegistryObjectList = [.. enumeratedEntriesResult];
        }

        var adhocQuery = AdhocQueries.FirstOrDefault(id => id.Value == adhocQueryRequest?.AdhocQuery.Id);

        _logger.LogInformation($"{soapEnvelope?.Header.MessageId} - Registry Stored Query Complete, returned {enumeratedEntriesResult?.Count ?? 0} XDSEntries for AdhocQuery Type {adhocQuery.Key} ({adhocQuery.Value})");
        return new SoapRequestResult<SoapEnvelope>() { Value = responseEnvelope, IsSuccess = true };
    }

    public SoapRequestResult<SoapEnvelope> DeleteDocumentSet(SoapEnvelope soapEnvelope, out IEnumerable<IdentifiableType> deletedObjects)
    {
        var registryResponse = new RegistryResponseType();
        var removeObjectsRequest = soapEnvelope.Body.RemoveObjectsRequest;

        var registryDtoContent = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var objectRefList = removeObjectsRequest?.ObjectRefList?.ObjectRef ?? [];
        var objectRefIds = objectRefList.Select(orl => orl.Id).ToHashSet();

        int removedDocumentsCount = 0;

        var objectsToRemove = registryDtoContent
            .Where(ro => objectRefIds.Contains(ro.Id))
            .ToList();

        deletedObjects = RegistryMetadataTransformerService.TransformRegistryObjectDtosToRegistryObjectsStateless(objectsToRemove);

        foreach (var registryObject in objectsToRemove)
        {
            if (objectRefIds.Contains(registryObject.Id))
            {
                var response = _registryWrapper.DeleteRegistryObjectFromRegistry(registryObject);
                if (response.IsSuccess)
                    removedDocumentsCount++;
                else
                    registryResponse.AddError(XdsErrorCodes.XDSRegistryError, $"Error while deleting document with Id '{registryObject.Id}'\n {response.Message}", "XDS Registry");
            }
        }

        // Skip if nothing was removed
        if (removedDocumentsCount == 0)
        {
            registryResponse.EvaluateStatusCode();
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        registryResponse.EvaluateStatusCode();
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> CopyIti41ToIti42Message(SoapEnvelope iti41Message)
    {
        var iti42Message = CopyIti41ToIti42Message(iti41Message.Body.ProvideAndRegisterDocumentSetRequest);
        iti42Message.Value ??= new();
        iti42Message.Value.Header = iti41Message.Header;
        return iti42Message;
    }

    public SoapRequestResult<SoapEnvelope> CopyIti41ToIti42Message(ProvideAndRegisterDocumentSetRequestType? provideAndRegisterRequest)
    {
        var registryResponse = new RegistryResponseType();

        var iti42Message = new SoapEnvelope()
        {
            Header = new() { Action = Constants.Xds.OperationContract.Iti42Action },
            Body = new() { RegisterDocumentSetRequest = new() { SubmitObjectsRequest = new() } }
        };

        iti42Message.Body.RegisterDocumentSetRequest.SubmitObjectsRequest = provideAndRegisterRequest?.SubmitObjectsRequest;

        var registryObjectList = iti42Message.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        var associations = registryObjectList?.OfType<AssociationType>().ToArray();
        var registryPackages = registryObjectList?.OfType<RegistryPackageType>().ToArray();
        var extrinsicObjects = registryObjectList?.OfType<ExtrinsicObjectType>().ToArray();
        var documents = provideAndRegisterRequest?.Document;

        foreach (var association in associations ?? [])
        {
            if (extrinsicObjects?.Length == 0 || registryPackages?.Length == 0) continue;

            if (association.AssociationTypeData != Constants.Xds.AssociationType.HasMember) continue;

            var assocExtrinsicObject = extrinsicObjects!.FirstOrDefault(eo => eo.Id?.NoUrn() == association.TargetObject?.NoUrn());
            var assocRegistryPackage = registryPackages!.FirstOrDefault(eo => eo.Id?.NoUrn() == association.SourceObject?.NoUrn());
            var assocDocument = documents?.FirstOrDefault(doc => doc.Id?.NoUrn() == assocExtrinsicObject?.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value?.NoUrn());

            if (assocExtrinsicObject == null && assocDocument != null && assocRegistryPackage == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingDocumentMetadata, $"Missing document metadata for document with ID {assocDocument?.Id}", "XDS Registry");
                continue;
            }

            if (assocExtrinsicObject == null || assocDocument == null || assocRegistryPackage == null)
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
            if (assocDocument.Value != null)
            {
                var documentHash = BitConverter.ToString(SHA1.HashData(assocDocument.Value)).Replace("-", "").ToLowerInvariant();

                // Check if the submissionset already has a hash, and if its the same as the calculated hash
                var extrinsicObjectHash = assocExtrinsicObject.GetSlots("Hash").FirstOrDefault()?.GetFirstValue();
                if (!string.IsNullOrWhiteSpace(extrinsicObjectHash) && extrinsicObjectHash != documentHash)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSNonIdenticalHash, "Document hash was not equal to hash value in extrinsic object", "XDS Registry");
                    return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
                }
                assocExtrinsicObject.AddSlot(Constants.Xds.SlotNames.Hash, [documentHash]);
            }

            // RepositoryUniqueId
            assocExtrinsicObject.AddSlot(Constants.Xds.SlotNames.RepositoryUniqueId, [_xdsConfig.RepositoryUniqueId]);

            // Document Size slot
            var documentSize = assocDocument.Value?.Length;
            assocExtrinsicObject.AddSlot(Constants.Xds.SlotNames.Size, [documentSize.ToString()]);

            // Document LegalAuthenticator slot
            var authorPersonSlot = assocExtrinsicObject.Classification?
                .FirstOrDefault(cl => cl.ClassificationScheme == Constants.Xds.Uuids.SubmissionSet.Author)?.Slot?
                .FirstOrDefault(sl => sl.Name == Constants.Xds.SlotNames.AuthorPerson);

            if (authorPersonSlot != null)
            {
                assocExtrinsicObject.AddSlot(authorPersonSlot);
            }


            // Switch from SubmissionSet UUIDs to XdsDocumentEntry UUIDs
            foreach (var classification in assocExtrinsicObject.Classification ?? [])
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

    private static bool DuplicateUuidsExist(IEnumerable<IdentifiableType> registryObjectList, IEnumerable<IdentifiableType> submissionRegistryObjects, out string[] duplicateIds)
    {
        var allObjects = registryObjectList.Concat(submissionRegistryObjects);

        var duplicates = allObjects
            .Where(obj => obj.Id != null)
            .GroupBy(obj => obj.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        duplicateIds = duplicates.OfType<string>().ToArray();

        return duplicateIds.Length > 0;
    }
}