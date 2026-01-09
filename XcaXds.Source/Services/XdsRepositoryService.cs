using Hl7.FhirPath.Sprache;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Buffers.Text;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
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

    public SoapRequestResult<SoapEnvelope> UploadContentToRepository(SoapEnvelope iti41Envelope)
    {
        return UploadContentToRepository(iti41Envelope.Body.ProvideAndRegisterDocumentSetRequest);
    }

    public SoapRequestResult<SoapEnvelope> UploadContentToRepository(ProvideAndRegisterDocumentSetRequestType? provideAndRegisterDocumentSetRequest)
    {
        var registryResponse = new RegistryResponseType();
        
        var registryObjectList = provideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;
        
        if (registryObjectList == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSStoredQueryMissingParam, "Missing RegistryObjectlist", _xdsConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var associations = registryObjectList.OfType<AssociationType>().ToArray();
        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var documents = provideAndRegisterDocumentSetRequest?.Document;

		// Only process HasMember associations (SubmissionSet pointing to a document) for document storage (others such as RPLC, XFRM etc. are not handled here)
		foreach (var association in associations.Where(a => a.AssociationTypeData == Constants.Xds.AssociationType.HasMember))
        {
            var assocDocument = documents?.FirstOrDefault(doc => doc.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id?.NoUrn() == association.TargetObject.NoUrn());            
			var assocRegistryPackage = registryPackages.FirstOrDefault(rp => rp.Id?.NoUrn() == association.SourceObject.NoUrn());

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
            var documentEntryUniqueId = assocExtrinsicObject?.ExternalIdentifier?.FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value;

            if (documentEntryUniqueId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Document unique ID missing", "ExtrinsicObject");
            }
            else
            {
                if (documentEntryUniqueId.NoUrn() == assocDocument?.Id.NoUrn())
                {
                    registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Unique ID in DocumentEntry does not match Document unique ID. DocumentEntry UniqueID: {documentEntryUniqueId}, Document ID: {assocDocument.Id}", $"XDS Repository");
                }

                if (assocDocument != null && assocDocument?.Value != null && !string.IsNullOrWhiteSpace(patientIdPart))
                {
                    var repositoryUpdateOk = _repositoryWrapper.StoreDocument(documentEntryUniqueId, assocDocument.Value, patientIdPart);
                    if (!repositoryUpdateOk)
                    {
                        registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Error while updating repository with document {assocDocument.Id}. Document name and patient ID must match Regex ^[a-zA-Z0-9\\-_\\.^]+$", $"XDS Repository");
                    }
                }

            }
        }
        registryResponse.EvaluateStatusCode();
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);

    }

    public SoapRequestResult<SoapEnvelope> CheckIfDocumentExistsInRepository(SoapEnvelope iti41Envelope)
    {
        return CheckIfDocumentExistsInRepository(iti41Envelope.Body.ProvideAndRegisterDocumentSetRequest);
    }

    public SoapRequestResult<SoapEnvelope> CheckIfDocumentExistsInRepository(ProvideAndRegisterDocumentSetRequestType? provideAndRegisterRequest)
    {
        var registryResponse = new RegistryResponseType();
        var documents = provideAndRegisterRequest?.Document;

        if (documents != null && documents.Length != 0)
        {
            foreach (var document in documents)
            {
                if (document != null && _repositoryWrapper.CheckIfFileExistsInRepository(document.Id.NoUrn()))
                {
                    registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Non unique ID in repository {document.Id}".Trim(), _xdsConfig.HomeCommunityId);
                }
            }
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> CheckIfDocumentsAreTooLarge(SoapEnvelope soapEnvelope)
    {
        return CheckIfDocumentsAreTooLarge(soapEnvelope.Body.ProvideAndRegisterDocumentSetRequest);
    }

    public SoapRequestResult<SoapEnvelope> CheckIfDocumentsAreTooLarge(ProvideAndRegisterDocumentSetRequestType? provideAndRegisterRequest)
    {
        var registryResponse = new RegistryResponseType();

        var oversizedDocuments = provideAndRegisterRequest?.Document
            .Where(doc => doc.Value.Length > (_xdsConfig.DocumentUploadSizeLimitKb * 1024)).ToList();

		// var oversizedDocuments = provideAndRegisterRequest?.Document.ToList(); Debug line to test oversize handling

		if (oversizedDocuments?.Count > 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Documents submitted are too large!\nIDs: {string.Join(", ", oversizedDocuments.Select(od => od.Id))}", _xdsConfig.HomeCommunityId);
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> RetrieveDocumentSet(SoapEnvelope iti43envelope)
    {
        var registryResponse = new RegistryResponseType();
        var retrieveResponse = new RetrieveDocumentSetResponseType();
        var iti43envelopeBody = iti43envelope.Body.RetrieveDocumentSetRequest;

        if (iti43envelopeBody == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing iti43envelopeBody in RetrieveDocumentSetRequest", _xdsConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var documentRequests = iti43envelopeBody.DocumentRequest;
        if (documentRequests == null || documentRequests.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RetrieveDocumentSetRequest", _xdsConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        foreach (var document in documentRequests)
        {
            var documentUniqueId = document.DocumentUniqueId;
            var repositoryUniqueId = document.RepositoryUniqueId?.NoUrn();
            var homeCommunityId = document.HomeCommunityId?.NoUrn();

            if (string.IsNullOrEmpty(documentUniqueId))
            {
                registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Missing document Id {documentUniqueId}".Trim(), _xdsConfig.HomeCommunityId);
                continue;
            }
            if (string.IsNullOrEmpty(homeCommunityId))
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingHomeCommunityId, $"Missing HomeCommunityID. Excpected {_xdsConfig.HomeCommunityId}", _xdsConfig.HomeCommunityId);
                continue;
            }
            if (homeCommunityId != _xdsConfig.HomeCommunityId)
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownCommunity, $"Unknown HomeCommunityID {homeCommunityId}".Trim(), _xdsConfig.HomeCommunityId);
                continue;
            }
            if (string.IsNullOrWhiteSpace(repositoryUniqueId) || repositoryUniqueId != _xdsConfig.RepositoryUniqueId)
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownRepositoryId, $"Unknown or missing repository ID {repositoryUniqueId}".Trim(), _xdsConfig.HomeCommunityId);
                continue;
            }

            var file = _repositoryWrapper.GetDocumentFromRepository(homeCommunityId, repositoryUniqueId, documentUniqueId);

            if (file != null && file.Length != 0)
            {
                var inputString = Encoding.UTF8.GetString(file);

                if (Base64.IsValid(inputString))
                {
                    var base64Document = Convert.FromBase64String(inputString);
                    file = new byte[base64Document.Length];
                    file = base64Document;
                }

                var mimeType = StringExtensions.GetMimetypeFromMagicNumber(file);
                if (mimeType == "application/pdf")
                {
                    try
                    {
                        using MemoryStream ms = new(file);

                        PdfDocument pdfDoc = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);

                        pdfDoc.Info.Title = documentUniqueId;

                        using MemoryStream outputStream = new();
                        pdfDoc.Save(outputStream);
                        pdfDoc.Close();

                        file = outputStream.ToArray();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{iti43envelope.Header.MessageId} \n" + ex.ToString());
                    }

                    // Get the modified file bytes and replace the original file variable
                }
                retrieveResponse.AddDocument(file, homeCommunityId, repositoryUniqueId, documentUniqueId, mimeType);
            }
        }

        registryResponse.EvaluateStatusCode();
        retrieveResponse.RegistryResponse = registryResponse;

        _logger.LogInformation($"{iti43envelope.Header.MessageId} - Retrieved {retrieveResponse?.DocumentResponse?.Length ?? 0} document(s)");

        for (int i = 0; i < retrieveResponse?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length; i++)
        {
            var error = retrieveResponse.RegistryResponse.RegistryErrorList?.RegistryError[i];
            if (error == null) continue;

            _logger.LogWarning($"{iti43envelope.Header.MessageId} - ERROR #{i+1}: Severity:{error.Severity}\n\t \n\t Code:{error.ErrorCode}\n\tCodeContext: {error.CodeContext}\n\tLocation: {error.Location}");
        }

        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            IsSuccess = true,
            Value = new SoapEnvelope()
            {
                Header = SoapExtensions.GetResponseHeaderFromRequest(iti43envelope),
                Body = new()
                {
                    RetrieveDocumentSetResponse = retrieveResponse
                }
            }
        };

        resultEnvelope.Value.Header.Action = iti43envelope.GetCorrespondingResponseAction();

        return resultEnvelope;
    }


    public SoapRequestResult<SoapEnvelope> RemoveDocuments(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();
        var removeDocuments = soapEnvelope.Body.RemoveDocumentsRequest?.DocumentRequest;

        if (removeDocuments == null || removeDocuments.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RemoveDocumentRequest", _xdsConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        foreach (var document in removeDocuments)
        {
            if (_xdsConfig.RepositoryUniqueId == document.RepositoryUniqueId)
            {
                if (document.DocumentUniqueId == null)
                {
                    registryResponse.AddWarning(XdsErrorCodes.XDSDocumentUniqueIdError, $"Missing document Id: {document.DocumentUniqueId}".Trim());
                    continue;
                }

                // Try to remove current document
                var removeResult = _repositoryWrapper.DeleteSingleDocument(document.DocumentUniqueId);

                if (removeResult == false)
                {
                    registryResponse.AddWarning(XdsErrorCodes.XDSDocumentUniqueIdError, $"Document not found. Id: {document.DocumentUniqueId}".Trim());
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

        if (registryResponse.Status == Constants.Xds.ResponseStatusTypes.Success)
        {
            _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Deleted {removeDocuments.Length} document(s)");
        }

        for (int i = 0; i < registryResponse.RegistryErrorList?.RegistryError.Length; i++)
        {
            var error = registryResponse.RegistryErrorList?.RegistryError[i];
            if (error == null) continue;

            _logger.LogWarning($"{soapEnvelope.Header.MessageId} - ERROR #{i + 1}: Severity:{error.Severity}\n\t \n\t Code:{error.ErrorCode}\n\tCodeContext: {error.CodeContext}\n\tLocation: {error.Location}");
        }

        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }
}
