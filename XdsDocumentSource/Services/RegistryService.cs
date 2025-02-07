using System.Security.Cryptography;
using XcaXds.Commons;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Hl7;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
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
            registryResponse.AddError(XdsErrorCodes.XDSDuplicateUniqueIdInRegistry, $"Duplicate UUIDs in request and registry {string.Join(", ",duplicateIds)}", "XDS Registry");
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);

        }


        // list spread to combine both lists
        registryContent.RegistryObjectList = [.. registryContent.RegistryObjectList, .. submissionRegistryObjects];

        var registryUpdatedOk = _registryWrapper.UpdateDocumentRegistry(registryContent);

        if (registryUpdatedOk)
        {
            registryResponse.SetSuccess();
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
        }
        registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Error while updating registry");
        return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> RegistryStoredQuery(SoapEnvelope soapEnvelope)
    {
        var adhocQueryRequest = soapEnvelope.Body.AdhocQueryRequest;
        var patientId = adhocQueryRequest.AdhocQuery.GetSlot(Constants.Xds.QueryParamters.DocumentEntry.PatientId).FirstOrDefault()?.GetFirstValue();
        var patient = Hl7Object.Parse<Xcn>(patientId.Trim('\''));

        if (patient.PersonIdentifier == null) return null;

        var registryContent = _registryWrapper.GetDocumentRegistryContent();
        var registryResponse = new RegistryResponseType();


        switch (adhocQueryRequest.AdhocQuery.Id)
        {
            case Constants.Xds.StoredQueries.FindDocuments:
                var patientIdValue = registryContent.RegistryObjectList.OfType<ExtrinsicObjectType>()
                    .SelectMany(eo => eo.ExternalIdentifier)
                    .FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.PatientId)?
                    .Value;

                var patientRegistryObjects = registryContent.RegistryObjectList.OfType<ExtrinsicObjectType>()
                    .Where(eo => eo.ExternalIdentifier
                    .Any(ei => ei.Value == patientIdValue))
                    .ToList();



                break;

        }


        throw new NotImplementedException();
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
