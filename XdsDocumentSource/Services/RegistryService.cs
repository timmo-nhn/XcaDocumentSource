using System.Security.Cryptography;
using XcaXds.Commons;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Hl7;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Xca;

namespace XcaXds.Source.Services;

public class RegistryService
{
    private readonly XdsConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    public RegistryService(XdsConfig xdsConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper)
    {
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
    }

    public SoapRequestResult<SoapEnvelope> CopyIti41ToIti42Message(SoapEnvelope iti41Message)
    {
        var iti42Message = new SoapEnvelope()
        {
            Header = iti41Message.Header,
            Body = new() { RegisterDocumentSetbRequest = new() { SubmitObjectsRequest = new() } }
        };

        iti42Message.Body.RegisterDocumentSetbRequest.SubmitObjectsRequest = iti41Message.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest;
        iti42Message.SetAction(Constants.Xds.OperationContract.Iti42Action);

        var registryResponse = new RegistryResponseType();

        var registryObjectList = iti42Message.Body.RegisterDocumentSetbRequest?.SubmitObjectsRequest.RegistryObjectList;

        var associations = registryObjectList.OfType<AssociationType>().ToArray();
        //var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var documentEntries = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var documents = iti41Message.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        foreach (var association in associations)
        {
            var assocDocument = documents.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());

            if (assocExtrinsicObject is null && assocDocument is not null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingDocumentMetadata, $"Missing document metadata for document with ID {assocDocument.Id}", "XDS Registry");
            }

            // Home attribute on extrinsicobject
            if (string.IsNullOrWhiteSpace(assocExtrinsicObject.Home))
            {
                assocExtrinsicObject.Home = _xdsConfig.HomeCommunityId;
            }

            // Document Hash slot
            var documentHash = Convert.ToHexString(SHA1.HashData(assocDocument.Value));

            // Check if the submissionset already has a hash, and if its the same as the calculated hash
            var extrinsicObjectHash = assocExtrinsicObject.GetSlot("Hash").FirstOrDefault()?.GetFirstValue();
            if (!string.IsNullOrWhiteSpace(extrinsicObjectHash) && extrinsicObjectHash != documentHash)
            {
                registryResponse.AddError(XdsErrorCodes.XDSNonIdenticalHash, "Document hash was not equal to hash value in extrinsic object", "XDS Registry");
                return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
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
        return new SoapRequestResult<SoapEnvelope>() { IsSuccess = true, Value = iti42Message };
    }

    public SoapRequestResult<SoapEnvelope> AppendToRegistry(SoapEnvelope envelope)
    {
        var registryResponse = new RegistryResponseType();
        var registryContent = _registryWrapper.GetDocumentRegistryContent();

        var submissionRegistryObjects = envelope.Body.RegisterDocumentSetbRequest?.SubmitObjectsRequest.RegistryObjectList.ToList();

        if (DuplicateUuidsExist(registryContent.RegistryObjectList, submissionRegistryObjects, out string[] duplicateIds))
        {
            registryResponse.AddError(XdsErrorCodes.XDSDuplicateUniqueIdInRegistry, $"Duplicate UUIDs in request and registry {string.Join(", ", duplicateIds)}", "XDS Registry");
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);

        }

        // list spread to combine both lists
        registryContent.RegistryObjectList = [.. registryContent.RegistryObjectList, .. submissionRegistryObjects];

        var registryUpdateResult = _registryWrapper.UpdateDocumentRegistry(registryContent);

        if (registryUpdateResult.IsSuccess)
        {
            registryResponse.SetStatusCode();
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
        }
        registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Error while updating registry");
        return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> RegistryStoredQuery(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();

        var adhocQueryRequest = soapEnvelope.Body.AdhocQueryRequest;
        var patientId = adhocQueryRequest?.AdhocQuery
            .GetSlot(Constants.Xds.QueryParamters.DocumentEntry.PatientId)
            .FirstOrDefault()?
            .GetFirstValue()?
            .Trim('\'');
        var patient = Hl7Object.Parse<Cx>(patientId.Trim('\''));

        if (patient == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSPatientIdDoesNotMatch, $"Missing or malformed patient ID {patientId}".Trim());
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
        }

        var registryContent = _registryWrapper.GetDocumentRegistryContent();

        var filteredElements = new List<IdentifiableType>();

        // Each "SlotType" in the AdhocQuery is treated as AND,
        // whilst the values in each slots valuelist is treated as OR
        // https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.5

        switch (adhocQueryRequest?.AdhocQuery.Id)
        {
            case Constants.Xds.StoredQueries.FindDocuments:
                var searchParameters = RegistryStoredQueryParameters.GetFindDocumentsParameters(adhocQueryRequest.AdhocQuery);

                var registryFindDocumentsResult = registryContent.RegistryObjectList
                    .OfType<ExtrinsicObjectType>();

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByPatientId(searchParameters.XdsDocumentEntryPatientId);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryClassCode(searchParameters.XdsDocumentEntryClassCode);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryTypeCode(searchParameters.XdsDocumentEntryTypeCode);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryPracticeSettingCode(searchParameters.XdsDocumentEntryPracticeSettingCode);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryCreationTimeFrom(searchParameters.XdsDocumentEntryCreationTimeFrom);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryCreationTimeTo(searchParameters.XdsDocumentEntryCreationTimeTo);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryServiceStartTimeFrom(searchParameters.XdsDocumentEntryServiceStartTimeFrom);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryServiceStartTimeTo(searchParameters.XdsDocumentEntryServiceStartTimeTo);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryServiceStopTimeFrom(searchParameters.XdsDocumentEntryServiceStoptimeFrom);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryServiceStopTimeTo(searchParameters.XdsDocumentEntryServiceStoptimeTo);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryHealthcareFacilityTypeCode(searchParameters.XdsDocumentEntryHealthcareFacilityTypeCode);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryEventCodeList(searchParameters.XdsDocumentEntryEventCodeList);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryConfidentialityCode(searchParameters.XdsDocumentEntryConfidentialityCode);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryAuthorPerson(searchParameters.XdsDocumentEntryAuthorPerson);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentFormatCode(searchParameters.XdsDocumentEntryFormatCode);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryStatus(searchParameters.XdsDocumentEntryStatus);

                registryFindDocumentsResult = registryFindDocumentsResult
                    .ByDocumentEntryType(searchParameters.XdsDocumentEntryType)
                    .ToList();
                filteredElements = [.. registryFindDocumentsResult];
                break;

            case Constants.Xds.StoredQueries.FindSubmissionSets:
                break;
            
            case Constants.Xds.StoredQueries.GetAssociations:
                break;
        }

        registryResponse.SetStatusCode();

        var responseEnvelope = new SoapEnvelope()
        {
            Header = new()
            {
                Action = soapEnvelope.GetCorrespondingResponseAction(),
                RelatesTo = soapEnvelope.Header.MessageId
            },
            Body = new()
            {
                RegistryResponse = registryResponse,
                AdhocQueryResponse = new() { RegistryObjectList = [.. filteredElements] }
            }
        };
        return new SoapRequestResult<SoapEnvelope>() { Value = responseEnvelope, IsSuccess = true };
        
    }

    public bool DuplicateUuidsExist(List<IdentifiableType> registryObjectList, List<IdentifiableType> submissionRegistryObjects, out string[] duplicateIds)
    {
        // Combine both lists into one
        var allObjects = registryObjectList.Concat(submissionRegistryObjects);

        // Group by ID and find duplicates
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
