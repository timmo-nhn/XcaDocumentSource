using System.Reflection;
using System.Security.Cryptography;
using XcaGatewayService.Commons;
using XcaGatewayService.Extensions;
using XcaGatewayService.Models.Soap;
using XcaGatewayService.Xca;
using XcaGatewayService.Commons.Enums;

namespace XcaGatewayService.Services;

public class RepositoryService
{
    private readonly XdsConfig _xdsConfig;
    private readonly XcaGateway _xcaGateway;
    private readonly RepositoryWrapper _repositoryWrapper;

    public RepositoryService(XdsConfig xdsConfig, XcaGateway xcaGateway, RepositoryWrapper repositoryWrapper)
    {
        _xdsConfig = xdsConfig;
        _xcaGateway = xcaGateway;
        _repositoryWrapper = repositoryWrapper;
    }
    public SoapRequestResult<SoapEnvelope> UploadFileToRepository(SoapEnvelope iti41Envelope)
    {
        var registryObjectList = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;

        var registryResponse = new RegistryResponseType();

        // Check! Will it even get here if registryobjectlist is missing?
        if (registryObjectList == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, "Missing RegistryObjectlist", "XDS Repository");
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
        }

        var associations = registryObjectList.OfType<AssociationType>().ToArray();
        //var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var documents = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        foreach (var association in associations)
        {
            var assocDocument = documents.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());

            var patientId = assocExtrinsicObject.Slot.FirstOrDefault(s => s.Name == "sourcePatientId")?.ValueList.Value.FirstOrDefault();
            var patientIdPart = patientId.Substring(0, patientId.IndexOf("^^^"));
            var xdsDocumentEntryUniqueId = assocExtrinsicObject.ExternalIdentifier.FirstOrDefault(ei => ei.Name.GetFirstValue() == Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId).Value;

            if (xdsDocumentEntryUniqueId != null)
            {
                xdsDocumentEntryUniqueId = _xdsConfig.RepositoryUniqueId + "^" + assocDocument.Id.NoUrn();
            }

            var repositoryUpdateOk = _repositoryWrapper.StoreDocument(assocDocument, patientIdPart);

            if (repositoryUpdateOk)
            {
                registryResponse.AddSuccess($"Document uploaded successfully({assocDocument.Id})");
            }
            else
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Error while updating repository",$"¨Document: {assocDocument.Id}");
            }

        }
        return SoapExtensions.CreateSoapRegistryResponse(registryResponse);

    }

}
