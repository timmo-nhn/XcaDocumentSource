using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Buffers.Text;
using System.Text;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.Shared.Extensions;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;
using XcaXds.WebService.Services.XdsRegistry;

namespace XcaXds.WebService.Services.XdsRepository;

public class XdsRepositoryService
{
    private readonly ILogger<XdsRepositoryService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly RegistryWrapper _registryWrapper;
    private readonly XdsSubmitObjectsValidator _submitObjectsValidator;
    private readonly IVirusScanner _fileScanner;
    private readonly BusinessLogicFiltersRegistry _businessLogicFiltersRegistry;
    private readonly TerminologyService _terminologyService;
    private readonly BusinessLogicMapperService _businessLogicMapperService;

    private ValueTuple<string, string>[] HealthcarePersonellCodesToRestrict { get; set; }
    private ValueTuple<string, string>[] CitizenCodesToRestrict { get; set; }

    public XdsRepositoryService(
        ApplicationConfig appConfig,
        RepositoryWrapper repositoryWrapper,
        RegistryWrapper registryWrapper,
        ILogger<XdsRepositoryService> logger,
        XdsSubmitObjectsValidator submitObjectsValidator,
        IVirusScanner fileScanner,
        BusinessLogicFiltersRegistry businessLogicFiltersRegistry,
        TerminologyService terminologyService,
        BusinessLogicMapperService businessLogicMapperService)
    {
        _logger = logger;
        _appConfig = appConfig;
        _submitObjectsValidator = submitObjectsValidator;
        _repositoryWrapper = repositoryWrapper;
        _registryWrapper = registryWrapper;
        _fileScanner = fileScanner;
        _businessLogicFiltersRegistry = businessLogicFiltersRegistry;
        _terminologyService = terminologyService;
        _businessLogicMapperService = businessLogicMapperService;

        HealthcarePersonellCodesToRestrict = _businessLogicFiltersRegistry.GetHealthcarePersonellConfidentialityCodesToObfuscate();
        CitizenCodesToRestrict = _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate();

    }

    public async Task<SoapRequestResult<SoapEnvelope>> UploadContentToRepository(SoapEnvelope iti41Envelope, bool validateOnly = false)
    {
        return await UploadContentToRepository(iti41Envelope.Body.ProvideAndRegisterDocumentSetRequest, validateOnly);
    }

    public async Task<SoapRequestResult<SoapEnvelope>> UploadContentToRepository(ProvideAndRegisterDocumentSetRequestType? provideAndRegisterDocumentSetRequest, bool validateOnly = false)
    {
        var registryResponse = new RegistryResponseType();

        var registryObjectList = provideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        if (registryObjectList == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSMissingDocumentMetadata, "Missing RegistryObjectlist", _appConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var associations = registryObjectList.OfType<AssociationType>().ToArray();
        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var documents = provideAndRegisterDocumentSetRequest?.Document;

        var documentsToUpload = new List<(DocumentType, string?)>();

        if (associations.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "No Associations in SubmitObjectsRequest, unable to determine RegistryObject relationships", "XDS Registry");
        }

        // Only process HasMember associations (SubmissionSet pointing to a document) for document storage (others such as RPLC, XFRM etc. are not handled here)
        foreach (var association in associations.Where(a => a.AssociationTypeData == Constants.Xds.AssociationType.HasMember))
        {
            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id?.NoUrn() == association.TargetObject?.NoUrn());
            var assocRegistryPackage = registryPackages.FirstOrDefault(rp => rp.Id?.NoUrn() == association.SourceObject?.NoUrn());
            var assocDocument = documents?.FirstOrDefault(doc => doc.Id?.NoUrn() == assocExtrinsicObject?.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value?.NoUrn());

            if (assocExtrinsicObject == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "ExtrinsicObject Missing", "SubmitObjectsRequest");
            }

            if (assocDocument == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Document missing", "SubmitObjectsRequest");
            }

            var patientId = assocExtrinsicObject?.Slot?.FirstOrDefault(s => s.Name == "sourcePatientId")?.ValueList?.Value?.FirstOrDefault();
            if (patientId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, "Patient ID missing", "ExtrinsicObject");
            }

            if (_appConfig.VirusScannerEnabled)
            {
                var scanResult = await _fileScanner.ScanFile(assocDocument?.Value ?? []);

                if (!scanResult.IsSuccess)
                {
                    _logger.LogWarning(scanResult.Message);
                    registryResponse.AddError(XdsErrorCodes.XDSRegistryError, scanResult.Message!, $"Document ({assocDocument?.Id})");
                }
            }

            var patientIdPart = Hl7Object.Parse<CX>(patientId)?.IdNumber;
            var documentEntryUniqueId = assocExtrinsicObject?.ExternalIdentifier?.FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value;

            var mimeTypeFromMagicByte = MimeTypeExtensions.TryGetMimeTypeFromDocumentBytes(assocDocument?.Value, out var mime) ? mime : null;
            var documentEntryMimetype = assocExtrinsicObject?.MimeType;

            if (!documentEntryMimetype.IsAnyOf(_businessLogicFiltersRegistry.GetAllowedMimeTypes()) ||
                !mimeTypeFromMagicByte.IsAnyOf(_businessLogicFiltersRegistry.GetAllowedMimeTypes()))
            {
                var message = $"Unsupported MimeType {mimeTypeFromMagicByte}";

                _logger.LogWarning(message);
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, message, "XDS Repository");
            }

            if (!BusinessLogicFiltersRegistry.IsMatchingMimeType(mimeTypeFromMagicByte, documentEntryMimetype))
            {
                var message = $"MimeType in DocumentEntry is missing or does not match actual document mime type. Document ID: {assocDocument?.Id}, DocumentEntry MimeType: {documentEntryMimetype}, Actual MimeType: {mimeTypeFromMagicByte}";

                _logger.LogWarning(message);
                registryResponse.AddError(XdsErrorCodes.XDSRegistryError, message, "XDS Repository");
            }

            if (documentEntryUniqueId == null)
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Document unique ID missing", "ExtrinsicObject");
            }

            if (documentEntryUniqueId?.NoUrn() != assocDocument?.Id?.NoUrn())
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Unique ID in DocumentEntry does not match Document unique ID. DocumentEntry UniqueID: {documentEntryUniqueId}, Document ID: {assocDocument?.Id}", $"XDS Repository");
            }

            if (assocDocument != null)
            {
                documentsToUpload.Add((assocDocument, patientId));
            }
        }

        _logger.LogInformation($"Document validation completed for {documentsToUpload.Count} document(s) ready for upload to repository");
        _logger.LogInformation($"RegistryErrorList contains {registryResponse.RegistryErrorList?.RegistryError.Length} entries");

        // We should not break the loop if any errors are found, but also never store any documents to maintain submission atomicity
        if ((registryResponse.RegistryErrorList?.RegistryError.Length > 0) == false)
        {
            foreach ((DocumentType assocDocument, string? patientIdPart) in documentsToUpload)
            {
                if (assocDocument == null || assocDocument.Id == null || assocDocument?.Value == null || string.IsNullOrWhiteSpace(patientIdPart))
                {
                    continue;
                }

                if (!validateOnly)
                {
                    var storeDocumentsResult = _repositoryWrapper.StoreDocument(assocDocument.Id, assocDocument.Value, patientIdPart);

                    if (storeDocumentsResult.IsSuccess == false)
                    {
                        registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Error while updating repository with document with ID '{assocDocument.Id}'. Message: {storeDocumentsResult.Message}", $"XDS Repository");
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

        if (documents?.Length > 0)
        {
            foreach (var document in documents)
            {
                if (document != null && _repositoryWrapper.FileExistsInRepository(document.Id?.NoUrn()))
                {
                    registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Non unique ID in repository {document.Id}".Trim(), _appConfig.HomeCommunityId);
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

        var oversizedDocuments = provideAndRegisterRequest?.Document?
            .Where(doc => doc.Value?.Length > (_appConfig.DocumentUploadSizeLimitKb * 1024)).ToList();

        // var oversizedDocuments = provideAndRegisterRequest?.Document.ToList(); Debug line to test oversize handling

        if (oversizedDocuments?.Count > 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Documents submitted are too large (max {_appConfig.DocumentUploadSizeLimitKb} KB per document)!\nIDs: {string.Join(", ", oversizedDocuments.Select(od => od.Id))}", _appConfig.HomeCommunityId);
        }
        return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
    }

    public SoapRequestResult<SoapEnvelope> RetrieveDocumentSet(SoapEnvelope iti43Envelope, AbacRequest? abacRequest = null)
    {
        var registryResponse = new RegistryResponseType();
        var retrieveResponse = new RetrieveDocumentSetResponseType();
        var iti43EnvelopeBody = iti43Envelope.Body.RetrieveDocumentSetRequest;

        if (iti43EnvelopeBody == null)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing RetrieveDocumentSetRequest", _appConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        var documentRequests = iti43EnvelopeBody.DocumentRequest;
        if (documentRequests == null || documentRequests.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RetrieveDocumentSetRequest", _appConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        foreach (var document in documentRequests)
        {
            var documentUniqueId = document.DocumentUniqueId;
            var repositoryUniqueId = document.RepositoryUniqueId?.NoUrn();
            var homeCommunityId = document.HomeCommunityId?.NoUrn();

            if (DocumentIsRestrictedForUser(document, abacRequest))
            {
                registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, $"Access denied for document {document.DocumentUniqueId}".Trim(), _appConfig.HomeCommunityId);
                continue;
            }

            if (string.IsNullOrEmpty(documentUniqueId))
            {
                registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Missing document Id {documentUniqueId}".Trim(), _appConfig.HomeCommunityId);
                continue;
            }
            if (string.IsNullOrEmpty(homeCommunityId))
            {
                registryResponse.AddError(XdsErrorCodes.XDSMissingHomeCommunityId, $"Missing HomeCommunityID. Excpected {_appConfig.HomeCommunityId}", _appConfig.HomeCommunityId);
                continue;
            }
            if (homeCommunityId != _appConfig.HomeCommunityId)
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownCommunity, $"Unknown HomeCommunityID {homeCommunityId}".Trim(), _appConfig.HomeCommunityId);
                continue;
            }
            if (string.IsNullOrWhiteSpace(repositoryUniqueId) || repositoryUniqueId != _appConfig.RepositoryUniqueId)
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownRepositoryId, $"Unknown or missing repository ID {repositoryUniqueId}".Trim(), _appConfig.HomeCommunityId);
                continue;
            }

            var file = _repositoryWrapper.GetDocumentFromRepository(homeCommunityId, repositoryUniqueId, documentUniqueId, out var documentKind, iti43Envelope.Header.MessageId);

            if (file?.Length > 0)
            {
                var inputString = Encoding.UTF8.GetString(file);

                if (Base64.IsValid(inputString))
                {
                    var base64Document = Convert.FromBase64String(inputString);
                    file = new byte[base64Document.Length];
                    file = base64Document;
                }

                var mimeType = MimeTypeExtensions.TryGetMimeTypeFromDocumentBytes(file, out var mime) ? mime : null;

                // If documentKind is unknown and the detected MimeType is pdf, then we actually have a proper PDF (Not CDA Wrapped) here.
                if (documentKind == DocumentSniffer.DocumentKind.Unknown && mimeType == "application/pdf")
                {
                    using MemoryStream ms = new(file);

                    PdfDocument pdfDoc = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);

                    pdfDoc.Info.Title = documentUniqueId;

                    using MemoryStream outputStream = new();
                    pdfDoc.Save(outputStream);
                    pdfDoc.Close();

                    file = outputStream.ToArray();
                }
                retrieveResponse.AddDocument(file, homeCommunityId, repositoryUniqueId, documentUniqueId, mimeType);
            }
        }

        registryResponse.EvaluateStatusCode();
        retrieveResponse.RegistryResponse = registryResponse;

        _logger.LogInformation($"{iti43Envelope.Header.MessageId} - Retrieved {retrieveResponse?.DocumentResponse?.Length ?? 0} document(s)");

        for (int i = 0; i < retrieveResponse?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length; i++)
        {
            var error = retrieveResponse.RegistryResponse.RegistryErrorList?.RegistryError[i];
            if (error == null) continue;

            _logger.LogWarning($"{iti43Envelope.Header.MessageId} - ERROR #{i + 1}: Severity:{error.Severity}\n\t \n\t Code:{error.ErrorCode}\n\tCodeContext: {error.CodeContext}\n\tLocation: {error.Location}");
        }

        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            IsSuccess = true,
            Value = new SoapEnvelope()
            {
                Header = SoapExtensions.GetResponseHeaderFromRequest(iti43Envelope),
                Body = new()
                {
                    RetrieveDocumentSetResponse = retrieveResponse
                }
            }
        };

        resultEnvelope.Value.Header.Action = iti43Envelope.GetCorrespondingResponseAction();

        return resultEnvelope;
    }

    private bool DocumentIsRestrictedForUser(DocumentRequestType document, AbacRequest? abacRequest)
    {
        var businessLogicParameters = _businessLogicMapperService.MapFromAbacRequestToBusinessLogic(abacRequest);

        var requestAppliesTo = businessLogicParameters.AppliesTo;

        var extrinsicObject = _registryWrapper.GetSingleRegistryObjectAsDto(document.DocumentUniqueId) as DocumentEntryDto;

        var confCodes = extrinsicObject?.ConfidentialityCode;

        bool restricted = requestAppliesTo switch
        {
            AppliesTo.HelseId => HealthcarePersonellCodesToRestrict.All(hcp => confCodes?.Any(ccode => ccode.Code == hcp.Item1 && ccode.CodeSystem == hcp.Item2) == true),
            AppliesTo.Helsenorge => CitizenCodesToRestrict.All(ccd => confCodes?.Any(ccode => ccode.Code == ccd.Item1 && ccode.CodeSystem == ccd.Item2) == true),
            _ => false
        };

        var purposeOfUseSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Authentication.PurposeOfUse);

        var ETREAT = purposeOfUseSystems.GetByValue("ETREAT")?.Value;
        var BTG = purposeOfUseSystems.GetByValue("BTG")?.Value;

        // Dont obscure in emergency situations
        if (restricted && !string.IsNullOrWhiteSpace(businessLogicParameters?.Purpose?.Code) && businessLogicParameters.Purpose.Code.IsAnyOf(ETREAT, BTG) == true)
        {
            restricted = false;
        }

        return restricted;
    }

    public SoapRequestResult<SoapEnvelope> RemoveDocuments(SoapEnvelope soapEnvelope)
    {
        var registryResponse = new RegistryResponseType();
        var removeDocuments = soapEnvelope.Body.RemoveDocumentsRequest?.DocumentRequest;

        if (removeDocuments == null || removeDocuments.Length == 0)
        {
            registryResponse.AddError(XdsErrorCodes.XDSRepositoryError, "Missing DocumentRequest in RemoveDocumentRequest", _appConfig.HomeCommunityId);
            return SoapExtensions.CreateSoapResultRegistryResponse(registryResponse);
        }

        foreach (var document in removeDocuments)
        {
            if (_appConfig.RepositoryUniqueId == document.RepositoryUniqueId)
            {
                if (document.DocumentUniqueId == null)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, $"Missing document ID '{document.DocumentUniqueId}'".Trim());
                    continue;
                }

                // Try to remove current document
                var removeResult = _repositoryWrapper.DeleteSingleDocument(document.DocumentUniqueId);

                if (removeResult.IsSuccess == false)
                {
                    registryResponse.AddError(XdsErrorCodes.XDSDocumentUniqueIdError, removeResult.Message ?? $"Error while trying to remove document with ID '{document.DocumentUniqueId}'".Trim());
                    continue;
                }
            }
            else
            {
                registryResponse.AddError(XdsErrorCodes.XDSUnknownRepositoryId, $"Unknown or missing RepositoryId or HomeCommunityId '{document.RepositoryUniqueId}'".Trim());
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
