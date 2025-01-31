using System.Xml;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("Repository/services")]
public class RepositoryController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;
    private readonly RepositoryService _repositoryService;
    private readonly RegistryService _registryService;

    public RepositoryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, RepositoryService repositoryService, RegistryService registryService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _repositoryService = repositoryService;
        _registryService = registryService;
    }

    public RegistryService RegistryService => _registryService;

    [Consumes("application/soap+xml")]
    [HttpPost("RepositoryService")]
    public async Task<IActionResult> HandlePostRequest([FromBody] SoapEnvelope soapEnvelope)
    {
        var xmlSerializerSettings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, IndentChars = "  " };
        var xmlSerializer = new SoapXmlSerializer(xmlSerializerSettings);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;

        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti43Action:
                //_repositoryService.GetFileFromRepository(soapEnvelope);
                break;
            case Constants.Xds.OperationContract.Iti41Action:
                var uploadResponse = _repositoryService.UploadFileToRepository(soapEnvelope);

                if (!uploadResponse.IsSuccess)
                {
                    return Ok(xmlSerializer.SerializeSoapMessageToXmlString(uploadResponse.Value));
                }
                var iti41Message = uploadResponse.Value;

                var iti42Message = RegistryService.ConvertIti41ToIti42Message(soapEnvelope);
                if (iti42Message.IsSuccess is false)
                {
                    return Ok(xmlSerializer.SerializeSoapMessageToXmlString(iti42Message.Value));
                }

                var registryResponse = await _xcaGateway.RegisterDocumentSetb(iti42Message.Value, baseUrl + "/Registry/services/RegistryService");
                
                var responseEnvelope = new SoapEnvelope()
                {
                    Header = new()
                    {
                        Action = soapEnvelope.GetCorrespondingResponseAction(),
                        RelatesTo = soapEnvelope.Header.MessageId
                    },
                    Body = new()
                    {
                        RegisterDocumentSetResponse = registryResponse.Value
                    }
                };

                return Ok(responseEnvelope);
        }

        if (action != null)
        {
            return Ok(SoapExtensions.CreateSoapFault("UnknownAction", $"Unknown action {action}").Value);
        }
        else
        {
            return Ok(SoapExtensions.CreateSoapFault("MissingAction", $"Missing action {action}".Trim()).Value);
        }

    }

}
