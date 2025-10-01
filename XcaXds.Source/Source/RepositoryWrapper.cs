using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
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

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntry = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.UniqueId == documentUniqueId);

        var submissionSet = documentRegistry
            .OfType<SubmissionSetDto>()
            .FirstOrDefault(ro => ro.Id == documentRegistry?
                .OfType<AssociationDto>()?
                .FirstOrDefault(assoc => assoc.TargetObject == documentEntry?.Id)?.SourceObject);

        var documentDto = new DocumentDto()
        {
            Data = _repository.Read(documentUniqueId),
            DocumentId = documentEntry?.Id
        };

        var cdaXml = string.Empty;

        var documentAsString = Encoding.UTF8.GetString(documentDto.Data ?? []);

        if (!string.IsNullOrWhiteSpace(documentAsString))
        {
            if (documentAsString.StartsWith("<ClinicalDocument") || GlobalExtensions.TryThis(() => { var xmlDocument = new XmlDocument(); xmlDocument.LoadXml(documentAsString);}))
            {
                _logger.LogInformation("CDA-wrapping skipped.. Document already in ClinicalDocument or XML format");
                cdaXml = documentAsString;
            }
            else
            {
                var clinicalDocument = CdaTransformerService.TransformRegistryObjectsToClinicalDocument(documentEntry, submissionSet, documentDto);
                cdaXml = sxmls.SerializeSoapMessageToXmlString(clinicalDocument, Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline).Content;
            }
        }

        return Encoding.UTF8.GetBytes(cdaXml);
    }

    public bool StoreDocument(string documentId, byte[] documentContent, string patientIdPart)
    {
        return _repository.Write(documentId, documentContent, patientIdPart);
    }

    public bool DeleteSingleDocument(string documentUniqueId)
    {
        return _repository.Delete(documentUniqueId);
    }

    public bool CheckIfFileExistsInRepository(string documentUniqueId)
    {
        return _repository.Read(documentUniqueId) != null;
    }

}
