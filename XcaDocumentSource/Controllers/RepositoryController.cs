using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models;
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
    private readonly XdsConfig _xdsConfig;

    public RepositoryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, RepositoryService repositoryService, RegistryService registryService, XdsConfig xdsConfig)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _repositoryService = repositoryService;
        _registryService = registryService;
        _xdsConfig = xdsConfig;
    }

    [Consumes("application/soap+xml")]
    [Produces("application/soap+xml",["application/xop+xml","application/octet-stream"])]
    [HttpPost("RepositoryService")]
    public async Task<IActionResult> HandlePostRequest([FromBody] SoapEnvelope soapEnvelope)
    {
        var xmlSerializer = new SoapXmlSerializer(XmlSettings.Soap);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;

        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti43Action:

                var documentFetchResponse = _repositoryService.GetContentFromRepository(soapEnvelope);

                if (documentFetchResponse.IsSuccess)
                {
                    return Ok(documentFetchResponse.Value);
                }
                break;

            case Constants.Xds.OperationContract.Iti41Action:

                var iti42Message = _registryService.CopyIti41ToIti42Message(soapEnvelope);
                if (iti42Message.IsSuccess is false)
                {
                    return Ok(iti42Message.Value);
                }

                var registerDocumentSetResponse = await _xcaGateway.RegisterDocumentSetb(iti42Message.Value, baseUrl + "/Registry/services/RegistryService");

                if (registerDocumentSetResponse.IsSuccess is false)
                {
                    return Ok(registerDocumentSetResponse.Value);
                }

                var responseEnvelope = new SoapEnvelope()
                {
                    Header = new()
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Action = soapEnvelope.GetCorrespondingResponseAction(),
                        RelatesTo = soapEnvelope.Header.MessageId
                    },
                    Body = new()
                    {
                        RegistryResponse = registerDocumentSetResponse.Value?.Body.RegistryResponse
                    }
                };

                if (iti42Message.IsSuccess is false)
                {
                    return Ok(responseEnvelope);
                }

                var uploadResponse = _repositoryService.UploadContentToRepository(soapEnvelope);

                if (!uploadResponse.IsSuccess)
                {
                    return Ok(uploadResponse.Value);
                }


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
