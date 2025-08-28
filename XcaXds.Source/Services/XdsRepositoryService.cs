using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Net.Http.Headers;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class XdsRepositoryService
{
    private readonly ApplicationConfig _xdsConfig;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly ILogger<XdsRepositoryService> _logger;


    public XdsRepositoryService(ApplicationConfig xdsConfig, RepositoryWrapper repositoryWrapper, ILogger<XdsRepositoryService> logger)
    {
        _xdsConfig = xdsConfig;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<SoapRequestResult<SoapEnvelope>> UploadContentToRepository(SoapEnvelope iti41Envelope)
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
        var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var documents = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        foreach (var association in associations)
        {
            var assocDocument = documents?.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocRegistryPackage = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.SourceObject.NoUrn());

            if (assocExtrinsicObject == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "ExtrinsicObject Missing", "SubmitObjectsRequest");
            }
            if (assocDocument == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Document missing", "SubmitObjectsRequest");
            }

            var patientId = assocExtrinsicObject?.Slot?.FirstOrDefault(s => s.Name == "sourcePatientId")?.ValueList.Value.FirstOrDefault();
            if (patientId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Patient ID missing", "ExtrinsicObject");
            }

            var patientIdPart = patientId?.Substring(0, patientId.IndexOf("^^^"));
            var xdsDocumentEntryUniqueId = assocExtrinsicObject?.ExternalIdentifier?.FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value;

            if (xdsDocumentEntryUniqueId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Document unique ID missing", "ExtrinsicObject");
            }
            else
            {
                xdsDocumentEntryUniqueId = assocDocument.Id.NoUrn();

                var repositoryUpdateOk = _repositoryWrapper.StoreDocument(xdsDocumentEntryUniqueId, assocDocument.Value, patientIdPart);

                if (repositoryUpdateOk)
                {
                }
                else
                {
                    registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Error while updating repository with document {assocDocument.Id}. Document name and patient ID must match Regex ^[a-zA-Z0-9\\-_\\.^]+$", $"XDS Repository");
                }
            }
        }
        registryResponse.EvaluateStatusCode();
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);

    }

    public async Task<SoapRequestResult<SoapEnvelope>> CheckIfDocumentExistsInRepository(SoapEnvelope iti41Envelope)
    {
        var registryResponse = new RegistryResponseType();
        var documents = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;

        var extrinsicObjects = iti41Envelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;



        if (documents != null && documents.Length != 0)
        {
            foreach (var document in documents)
            {
                if (document != null && _repositoryWrapper.CheckIfFileExistsInRepository(document.Id.NoUrn()))
                {
                    registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Non unique ID in repository {document.Id}".Trim(), "XDS Repository");
                }
            }
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public async Task<SoapRequestResult<SoapEnvelope>> CheckIfDocumentsAreTooLarge(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();

        var oversizedDocuments = soapEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.Document
            .Where(doc => doc.Value.Length > _xdsConfig.DocumentUploadSizeLimitKb).ToList();

        if (oversizedDocuments?.Count > 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Documents submitted are too large!\nIDs: {string.Join(", ", oversizedDocuments.Select(od => od.Id))}", "XDS Repository");
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public async Task<SoapRequestResult<SoapEnvelope>> RetrieveDocumentSet(SoapEnvelope iti43envelope)
    {
        var registryResponse = new RegistryResponseType();
        var retrieveResponse = new RetrieveDocumentSetResponseType();
        var iti43envelopeBody = iti43envelope.Body.RetrieveDocumentSetRequest;

        if (iti43envelopeBody == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing iti43envelopeBody in RetrieveDocumentSetRequest", "XDS Repository");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var documentRequests = iti43envelopeBody.DocumentRequest;
        if (documentRequests == null || documentRequests.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RetrieveDocumentSetRequest", "XDS Repository");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        foreach (var document in documentRequests)
        {
            var docId = document.DocumentUniqueId;
            var repoId = document.RepositoryUniqueId;
            var home = document.HomeCommunityId;

            if (string.IsNullOrEmpty(docId))
            {
                registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Missing document Id {docId}".Trim(), "XDS Repository");
                continue;
            }
            if (home != _xdsConfig.HomeCommunityId || string.IsNullOrEmpty(home))
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingHomeCommunityId, $"Missing or unknown HomeCommunityID {home}".Trim(), "XDS Repository");
                continue;
            }
            if (repoId != _xdsConfig.RepositoryUniqueId)
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownRepositoryId, $"Unknown repository ID {repoId}".Trim(), "XDS Repository");
                continue;
            }

            var file = _repositoryWrapper.GetDocumentFromRepository(home, repoId, docId);

            byte[] renamedFile;

            if (file != null)
            {
                var mimeType = StringExtensions.GetMimetypeFromMagicNumber(file);
                if (mimeType == "application/pdf")
                {
                    try
                    {
                        using MemoryStream ms = new(file);

                        PdfDocument pdfDoc = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);

                        pdfDoc.Info.Title = docId;

                        using MemoryStream outputStream = new();
                        pdfDoc.Save(outputStream);
                        pdfDoc.Close();

                        file = outputStream.ToArray();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                    }

                    // Get the modified file bytes and replace the original file variable
                }
                retrieveResponse.AddDocument(file, home, repoId, docId, mimeType);
            }
        }

        registryResponse.EvaluateStatusCode();
        retrieveResponse.RegistryResponse = registryResponse;

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


    public async Task<SoapRequestResult<SoapEnvelope>> RemoveDocuments(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();
        var removeDocuments = soapEnvelope.Body.RemoveDocumentsRequest?.DocumentRequest;

        if (removeDocuments == null || removeDocuments.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RemoveDocumentRequest", "XDS Repository");
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

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

    public MultipartContent ConvertToMultipartResponse(SoapEnvelope soapEnvelope)
    {
        var multipart = new MultipartContent("related", "boundary_" + Guid.NewGuid());
        multipart.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "\"application/xop+xml\""));

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        // Attach documents
        var documents = soapEnvelope.Body.RetrieveDocumentSetResponse?.DocumentResponse;
        var documentContents = new List<ByteArrayContent>();
    
        if (documents != null)
        {
            foreach (var document in documents)
            {
                // Generate a unique Content-ID for the document part
                var contentId = $"<{Guid.NewGuid()}@tempuri.org>";

                // Replace inline base64 with an XOP Include
                document.Include = new IncludeType
                {
                    href = $"cid:{contentId.Trim('<', '>')}" // cid: without angle brackets
                };

                // Add the binary part to multipart
                var byteContent = new ByteArrayContent(document.Document);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue(document.MimeType);
                byteContent.Headers.Add("Content-ID", contentId);
                byteContent.Headers.Add("Content-Transfer-Encoding", "binary");
                document.Document = null;

                documentContents.Add(byteContent);
            }
        }

        // Create soap message string now that document content has been removed from the soapenvelope object and put into the xop:Includes
        var soapString = sxmls.SerializeSoapMessageToXmlString(soapEnvelope).Content;

        var soapContent = new StringContent(soapString, Encoding.UTF8, Constants.MimeTypes.SoapXml);
        soapContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("charset", "\"utf-8\""));
        soapContent.Headers.Add("Content-ID", "<root.message@cxf.apache.org>"); // typical root part

        multipart.Add(soapContent);

        foreach (var docContent in documentContents)
        {
            multipart.Add(docContent);
        }


        return multipart;
    }
}
