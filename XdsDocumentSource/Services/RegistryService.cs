using System.Security.Cryptography;
using XdsDocumentSource.Commons;
using XdsDocumentSource.Commons.Enums;
using XdsDocumentSource.Extensions;
using XdsDocumentSource.Models.Soap;
using XdsDocumentSource.Xca;

namespace XdsDocumentSource.Services;

public class RegistryService
{
    private readonly XdsConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    public RegistryService(XdsConfig xdsConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper)
    {
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
    }

    public SoapRequestResult<SoapEnvelope> ConvertIti41ToIti42Message(SoapEnvelope iti41Message)
    {
        var iti42Message = iti41Message;

        var registerDocumentSet = new RegisterDocumentSetRequestType();
        var registryResponse = new RegistryResponseType();

        registerDocumentSet.SubmitObjectsRequest = iti41Message.Body.ProvideAndRegisterDocumentSetRequest.SubmitObjectsRequest;
        var registryObjectList = registerDocumentSet.SubmitObjectsRequest.RegistryObjectList;

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
                registryResponse.AddError(XdsErrorCodes.XDSMissingDocumentMetadata, $"Missing document metadata for document with ID {assocDocument.Id}", "XDS repository");
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
                registryResponse.AddError(XdsErrorCodes.XDSNonIdenticalHash, "Document hash was not equal to hash value in extrinsic object");
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

            // Preserve headers and stuff form ITI-41, and replace body with a register document set request
            iti42Message.Body.ProvideAndRegisterDocumentSetRequest = null;
            iti42Message.Body.RegisterDocumentSetRequest = registerDocumentSet;
            iti42Message.SetAction(Constants.Xds.OperationContract.Iti42Action);
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

        var extrinsicObjects = envelope.Body.RegisterDocumentSetRequest.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().ToList();
        // list spread to combine both lists
        registryContent.ExtrinsicObjects = [.. registryContent.ExtrinsicObjects, .. extrinsicObjects];

        var registryUpdatedOk = _registryWrapper.UpdateDocumentRegistry(registryContent);

        if (registryUpdatedOk)
        {
            registryResponse.AddSuccess("Registry uploaded successfully");
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
        }
        registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Error while updating registry");
        return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> RegistryStoredQuery(SoapEnvelope soapEnvelope)
    {
        throw new NotImplementedException();
    }
}
