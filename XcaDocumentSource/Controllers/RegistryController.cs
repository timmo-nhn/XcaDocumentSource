using System.Xml;
using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons;
using XcaXds.Commons.Enums;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("Registry/services")]
public class RegistryController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;
    private readonly RegistryService _registryService;

    public RegistryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, RegistryService registryService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _registryService = registryService;
    }

    [Consumes("application/soap+xml")]
    [Produces("application/soap+xml")]
    [HttpPost("RegistryService")]
    public async Task<IActionResult> RegistryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;
        var registryResponse = new RegistryResponseType();
        var xmlSerializerSettings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, IndentChars = "  " };
        var xmlSerializer = new SoapXmlSerializer(xmlSerializerSettings);

        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
                _registryService.RegistryStoredQuery(soapEnvelope);
                break;

            case Constants.Xds.OperationContract.Iti42Action:

                var registryUploadResponse = _registryService.AppendToRegistry(soapEnvelope);

                if (registryUploadResponse.IsSuccess)
                {
                    _logger.LogInformation("Registry updated successfully");
                    registryResponse.AddSuccess("Registry updated successfully");

                    var responseEnvelope = new SoapEnvelope()
                    {
                        Header = new()
                        {
                            Action = soapEnvelope.GetCorrespondingResponseAction(),
                            RelatesTo = soapEnvelope.Header.MessageId
                        },
                        Body = new() 
                        { 
                            RegisterDocumentSetbResponse = new() 
                            {
                                RegistryResponse = registryResponse 
                            }
                        }
                    };

                    return Ok(responseEnvelope);
                }
                else
                {
                    _logger.LogError("Error while updating registry", registryUploadResponse.Value.Body.Fault);
                    registryResponse.AddError(XdsErrorCodes.XDSRegistryError,"Error while updating registry " + registryUploadResponse.Value.Body.Fault, "XDS Registry");
                    
                    var responseEnvelope = new SoapEnvelope()
                    {
                        Header = new()
                        {
                            Action = soapEnvelope.GetCorrespondingResponseAction(),
                            RelatesTo = soapEnvelope.Header.MessageId
                        },
                        Body = new()
                        {
                            RegisterDocumentSetbResponse = new()
                            {
                                RegistryResponse = registryResponse
                            }
                        }
                    };
                    return Ok(responseEnvelope);
                }
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
