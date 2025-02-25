using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Text;
using XcaXds.Commons;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;

namespace XcaXds.Source.Services;

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
    public SoapRequestResult<SoapEnvelope> UploadContentToRepository(SoapEnvelope iti41Envelope)
    {
        var registryObjectList = iti41Envelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;

        var registryResponse = new RegistryResponseType();

        if (registryObjectList == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, "Missing RegistryObjectlist", "XDS Repository");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var associations = registryObjectList.OfType<AssociationType>().ToArray();
        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var documents = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        foreach (var association in associations)
        {
            var assocDocument = documents.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());

            var patientId = assocExtrinsicObject?.Slot?.FirstOrDefault(s => s.Name == "sourcePatientId")?.ValueList.Value.FirstOrDefault();
            if (patientId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryBusy, "Patient ID missing", "ExtrinsicObject");
                return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
            }

            var patientIdPart = patientId.Substring(0, patientId.IndexOf("^^^"));
            var xdsDocumentEntryUniqueId = assocExtrinsicObject?.ExternalIdentifier?.FirstOrDefault(ei => ei.Name.GetFirstValue() == Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId)?.Value;

            if (xdsDocumentEntryUniqueId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Document unique ID missing", "ExtrinsicObject");
                return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
            }

            xdsDocumentEntryUniqueId = assocDocument.Id.NoUrn();

            var repositoryUpdateOk = _repositoryWrapper.StoreDocument(xdsDocumentEntryUniqueId, assocDocument.Value, patientIdPart);

            if (repositoryUpdateOk)
            {
                registryResponse.EvaluateStatusCode();
            }
            else
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Error while updating repository with document {assocDocument.Id}. Document name and patient ID must match Regex ^[a-zA-Z0-9\\-_\\.^]+$", $"XDS Repository");
            }
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);

    }

    public SoapRequestResult<SoapEnvelope> CheckIfDocumentExistsInRepository(SoapEnvelope iti41Envelope)
    {
        var registryResponse = new RegistryResponseType();
        var documents = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        if (documents != null && documents.Length != 0)
        {
            foreach (var document in documents)
            {
                if (document != null && _repositoryWrapper.CheckIfFileExistsInRepository(document.Id.NoUrn()))
                {
                    registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Non unique ID in repository {document.Id}".Trim(), "XDS Repository");
                }
            }
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }


    public SoapRequestResult<SoapEnvelope> GetContentFromRepository(SoapEnvelope iti43envelope)
    {
        var registryResponse = new RegistryResponseType();
        var retrieveResponse = new RetrieveDocumentSetResponseType();
        var documentRequests = iti43envelope.Body.RetrieveDocumentSetRequest?.DocumentRequest;
        if (documentRequests == null || documentRequests?.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RetrieveDocumentSetRequest", "XDS Repository");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        foreach (var document in documentRequests)
        {
            var docId = document.DocumentUniqueId;
            var repoId = document.RepositoryUniqueId;
            var home = document.HomeCommunityId;

            if (home != _xdsConfig.HomeCommunityId || string.IsNullOrEmpty(home))
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingHomeCommunityId, $"Missing or unknown HomecommunityID {home}".Trim(), "XDS Repository");
                continue;
            }
            if (repoId != _xdsConfig.RepositoryUniqueId)
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownRepositoryId, $"Unknown repository ID {repoId}".Trim(),"XDS Repository");
                continue;
            }
            var file = _repositoryWrapper.GetDocumentFromRepository(home, repoId, docId);

            if (file != null)
            {
                retrieveResponse.AddDocument(file, home, repoId, docId);
            }
        }


        registryResponse.EvaluateStatusCode();
        retrieveResponse.RegistryResponse = registryResponse;

        var documentResponse = new RetrieveDocumentSetbResponse() 
        {
            RetrieveDocumentSetResponse = retrieveResponse
        };

        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            IsSuccess = true,
            Value = new SoapEnvelope()
            {
                Header = iti43envelope.Header,
                Body = new()
                {
                    RetrieveDocumentSetResponse = retrieveResponse
                }
            }
        };

        resultEnvelope.Value.Header.Action = iti43envelope.GetCorrespondingResponseAction();

        return resultEnvelope;
    }

    public MultipartContent ConvertToMultipartResponse(SoapEnvelope soapEnvelope)
    {
        var documents = soapEnvelope.Body.RetrieveDocumentSetResponse?.DocumentResponse;

        var multipart = new MultipartContent("related", Guid.NewGuid().ToString());
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        multipart.Headers.ContentType = new MediaTypeHeaderValue(Constants.MimeTypes.MultipartRelated);

        if (documents != null)
        {
            foreach (var document in documents)
            {
                var multipartInclude = new IncludeType()
                {
                    href = document.RepositoryUniqueId + document.DocumentUniqueId
                };

                var include = sxmls.SerializeSoapMessageToXmlString(multipartInclude);

                var documentBytes = document.Document;
                document.Include = multipartInclude;
                document.Document = null;
                Console.WriteLine();

                var documentString = Encoding.UTF8.GetString(documentBytes);

                multipart.Add(new StringContent(documentString, Encoding.UTF8, document.MimeType));
            }
        }

        var soapString = sxmls.SerializeSoapMessageToXmlString(soapEnvelope);

        multipart.Add(new StringContent(soapString.Content, Encoding.UTF8, Constants.MimeTypes.SoapXml));


        return multipart;
    }

    public SoapRequestResult<SoapEnvelope> RemoveDocuments(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();
        var removeDocuments = soapEnvelope.Body.RemoveDocumentsRequest.DocumentRequest;

        foreach (var document in removeDocuments)
        {
            if (_xdsConfig.HomeCommunityId == document.HomeCommunityId && 
                _xdsConfig.RepositoryUniqueId == document.RepositoryUniqueId)
            {
                if (document.DocumentUniqueId is null)
                {
                    registryResponse.AddWarning(XdsErrorCodes.XDSDocumentUniqueIdError, $"Missing document Id {document.DocumentUniqueId}".Trim());
                    continue;
                }

                // Try to remove current document
                var removeResult = _repositoryWrapper.DeleteSingleDocument(document.DocumentUniqueId);

                if (removeResult is false)
                {
                    registryResponse.AddWarning(XdsErrorCodes.XDSDocumentUniqueIdError, $"Document not found {document.DocumentUniqueId}".Trim());
                    continue;
                }
            }
            else
            {
                registryResponse.AddWarning(XdsErrorCodes.XDSUnknownRepositoryId, $"Unknown or missing RepositoryId or HomeCommunityId {document.RepositoryUniqueId}".Trim());
                continue;
            }

        }
        registryResponse.EvaluateStatusCode();
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }
}
