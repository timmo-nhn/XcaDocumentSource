using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;

namespace XcaXds.Source.Source;

public partial class RepositoryWrapper
{
    private readonly ApplicationConfig _appConfig;

    private readonly RegistryWrapper _registryWrapper;

    private readonly IRepository _repository;

    private ILogger<RepositoryWrapper> _logger;

    public RepositoryWrapper(ApplicationConfig appConfig, IRepository repository, RegistryWrapper registryWrapper, ILogger<RepositoryWrapper> logger)
    {
        _repository = repository;
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _logger = logger;
    }

    public byte[]? GetDocumentFromRepository(string homeCommunityId, string repositoryUniqueId, string documentUniqueId)
    {
        homeCommunityId = homeCommunityId.NoUrn();
        repositoryUniqueId = repositoryUniqueId.NoUrn();

        if (_appConfig.HomeCommunityId != homeCommunityId)
        {
            _logger.LogInformation($"Got document request with invalid HomeCommunityId {homeCommunityId}, Expected: {_appConfig.HomeCommunityId} ");
            return null;
        }

        if (repositoryUniqueId.Substring(repositoryUniqueId.LastIndexOf('/') + 1) != _appConfig.RepositoryUniqueId)
        {
            _logger.LogInformation($"Got document request with invalid RepositoryUniqueId {repositoryUniqueId}, Expected: {_appConfig.RepositoryUniqueId}");
            return null;
        }

        if (_appConfig.WrapRetrievedDocumentInCda == false)
        {
            return _repository.Read(documentUniqueId);
        }

        _logger.LogInformation($"WrapRetrievedDocumentInCda Enabled");


        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);


        var documentDto = new DocumentDto()
        {
            Data = _repository.Read(documentUniqueId),
            DocumentId = documentUniqueId
        };

        var cdaXml = string.Empty;        

		var kind = DocumentSniffer.DetectKind(documentDto.Data);

		if (kind == DocumentSniffer.DocumentKind.ClinicalDocumentXml || kind == DocumentSniffer.DocumentKind.Xml)
		{
			_logger.LogInformation("CDA-wrapping skipped.. Document already in ClinicalDocument XML format or XML format");
			cdaXml = Encoding.UTF8.GetString(documentDto.Data);
		}		
		else
		{
			// Not XML -> wrap into ClinicalDocument
			var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

			var documentEntry = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(ro => ro?.UniqueId == documentUniqueId);
			var associations = documentRegistry.OfType<AssociationDto>().FirstOrDefault(assoc => assoc?.TargetObject == documentEntry?.Id);
			var submissionSet = documentRegistry.OfType<SubmissionSetDto>().FirstOrDefault(ss => associations?.SourceObject == ss.Id);

			var clinicalDocument = CdaTransformerService.TransformRegistryObjectsToClinicalDocument(documentEntry, submissionSet, documentDto);
			cdaXml = sxmls.SerializeSoapMessageToXmlString(clinicalDocument, Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline).Content;
		}
	
        return Encoding.UTF8.GetBytes(cdaXml);
    }

    public bool StoreDocument(string documentId, byte[] documentContent, string patientIdPart)
    {
        return _repository.Write(documentId, documentContent, patientIdPart);
    }

    public bool DeleteSingleDocument(string? documentUniqueId)
    {
        return _repository.Delete(documentUniqueId);
    }

    public bool CheckIfFileExistsInRepository(string? documentUniqueId)
    {
        return _repository.Read(documentUniqueId) != null;
    }

    public bool SetNewRepositoryOid(string repositoryUniqueId, out string oldId)
    {
        return _repository.SetNewOid(repositoryUniqueId, out oldId);
    }
}
