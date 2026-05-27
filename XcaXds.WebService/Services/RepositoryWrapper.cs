using nClam;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Serializers;

namespace XcaXds.WebService.Services;

public partial class RepositoryWrapper
{
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly IRepository _repository;
    private readonly ILogger<RepositoryWrapper> _logger;
    private readonly IClamAvFileScanner _fileScanner;

    public RepositoryWrapper(ApplicationConfig appConfig, IRepository repository, RegistryWrapper registryWrapper, ILogger<RepositoryWrapper> logger, IClamAvFileScanner fileScanner)
    {
        _repository = repository;
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _logger = logger;
        _fileScanner = fileScanner;
    }

    public byte[]? GetDocumentFromRepository(string? homeCommunityId, string? repositoryUniqueId, string? documentUniqueId, string? messageId = null)
    {
        return GetDocumentFromRepository(homeCommunityId, repositoryUniqueId, documentUniqueId, out _, messageId);
    }

    public byte[]? GetDocumentFromRepository(string? homeCommunityId, string? repositoryUniqueId, string? documentUniqueId, out DocumentSniffer.DocumentKind documentKind, string? messageId = null)
        {
        documentKind = DocumentSniffer.DocumentKind.Unknown;

        homeCommunityId = homeCommunityId?.NoUrn();
        repositoryUniqueId = repositoryUniqueId?.NoUrn();

        if (_appConfig.HomeCommunityId != homeCommunityId)
        {
            _logger.LogInformation($"{messageId} - Got document request with invalid HomeCommunityId {homeCommunityId}, Expected: {_appConfig.HomeCommunityId} ".TrimStart([' ', '-']));
            return null;
        }

        if (repositoryUniqueId?.Substring(repositoryUniqueId.LastIndexOf('/') + 1) != _appConfig.RepositoryUniqueId)
        {
            _logger.LogInformation($"{messageId} - Got document request with invalid RepositoryUniqueId {repositoryUniqueId}, Expected: {_appConfig.RepositoryUniqueId}".TrimStart([' ', '-']));
            return null;
        }

        if (documentUniqueId == null)
        {
            _logger.LogInformation($"{messageId} - No documentUniqueId specified");
            return null;
        }

        if (_appConfig.WrapRetrievedDocumentInCda == false)
        {
            return _repository.Read(documentUniqueId);
        }

        _logger.LogInformation($"{messageId} - {nameof(_appConfig.WrapRetrievedDocumentInCda)} Enabled".TrimStart([' ', '-']));


        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);


        var documentDto = new DocumentDto()
        {
            Data = _repository.Read(documentUniqueId),
            DocumentId = documentUniqueId
        };

        var cdaXml = string.Empty;

        documentKind = DocumentSniffer.DetectKind(documentDto.Data);

        _logger.LogInformation($"{messageId} - Document kind {documentKind.ToString()}");

        if (documentKind == DocumentSniffer.DocumentKind.ClinicalDocumentXml)
        {
            _logger.LogInformation($"{messageId} - CDA-wrapping skipped.. Document already in ClinicalDocument XML format".TrimStart([' ', '-']));
            cdaXml = Encoding.UTF8.GetString(documentDto.Data ?? []);
        }
        else
        {
            // Not XML -> wrap into ClinicalDocument
            var documentEntries = _registryWrapper.GetRegistryItemAndRelated(documentUniqueId);

            var clinicalDocument = CdaTransformer.TransformRegistryObjectsToClinicalDocument(documentEntries?.OfType<DocumentEntryDto>().FirstOrDefault(), documentEntries?.OfType<SubmissionSetDto>().FirstOrDefault(), documentDto);

            cdaXml = sxmls.SerializeSoapMessageToXmlString(clinicalDocument, Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline).Content ??
                throw new InvalidOperationException("ClinicalDocument transformation resulted in empty ClinicalDocument");

            documentKind = DocumentSniffer.DocumentKind.ClinicalDocumentXml;
        }
        

        return Encoding.UTF8.GetBytes(cdaXml);
    }

    public bool StoreDocument(string documentId, byte[] documentContent, string patientIdPart)
    {
        var result = _repository.Write(documentId, documentContent, patientIdPart);
        return result;
    }

    public bool DeleteSingleDocument(string? documentUniqueId)
    {
        return _repository.Delete(documentUniqueId);
    }

    public bool FileExistsInRepository(string? documentUniqueId)
    {
        return _repository.Read(documentUniqueId ?? "") != null;
    }

    public bool SetNewRepositoryOid(string repositoryUniqueId, out string? oldId)
    {
        return _repository.SetNewOid(repositoryUniqueId, out oldId);
    }
}
