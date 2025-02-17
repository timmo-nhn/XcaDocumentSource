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

            var patientId = assocExtrinsicObject?.Slot?.FirstOrDefault(s => s.Name == "sourcePatientId")?.ValueList.Value.FirstOrDefault();
            if (patientId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryBusy, "Patient ID missing", "ExtrinsicObject");
                return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
            }

            var patientIdPart = patientId.Substring(0, patientId.IndexOf("^^^"));
            var xdsDocumentEntryUniqueId = assocExtrinsicObject?.ExternalIdentifier?.FirstOrDefault(ei => ei.Name.GetFirstValue() == Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId)?.Value;

            if (xdsDocumentEntryUniqueId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Document unique ID missing", "ExtrinsicObject");
                return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
            }

            xdsDocumentEntryUniqueId = _xdsConfig.RepositoryUniqueId + "^" + assocDocument.Id.NoUrn();

            var repositoryUpdateOk = _repositoryWrapper.StoreDocument(assocDocument, patientIdPart);

            if (repositoryUpdateOk)
            {
                registryResponse.SetStatusCode();
            }
            else
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Error while updating repository with document {assocDocument.Id}", $"XDS Repository");
            }
        }
        return SoapExtensions.CreateSoapRegistryResponse(registryResponse);

    }

    public SoapRequestResult<SoapEnvelope> GetContentFromRepository(SoapEnvelope iti43envelope)
    {
        var registryResponse = new RegistryResponseType();
        var retrieveResponse = new RetrieveDocumentSetResponseType();
        var documentRequests = iti43envelope.Body.RetrieveDocumentSetRequest?.DocumentRequest;
        if (documentRequests == null || documentRequests?.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RetrieveDocumentSetRequest", "XDS Repository");
            return SoapExtensions.CreateSoapRegistryResponse(registryResponse);
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


        registryResponse.SetStatusCode();
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
        var soapString = sxmls.SerializeSoapMessageToXmlString(soapEnvelope);

        multipart.Add(new StringContent(soapString.Content, Encoding.UTF8, Constants.MimeTypes.SoapXml));


        return multipart;
    }

    public void TryRemoveDocument(SoapEnvelope soapEnvelope)
    {
        //_repositoryWrapper.
        throw new NotImplementedException();
    }
}
