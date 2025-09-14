using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Net;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Controllers;

[Tags("SOAP Endpoints (IHE XDS/XCA)")]
[ApiController]
[Route("Repository/services")]
[UsePolicyEnforcementPoint]
public class XdsRepositoryController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly XdsRepositoryService _repositoryService;
    private readonly XdsRegistryService _registryService;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IVariantFeatureManager _featureManager;

    public XdsRepositoryController(ILogger<XdsRegistryController> logger, XdsRepositoryService repositoryService, XdsRegistryService registryService, ApplicationConfig xdsConfig, IVariantFeatureManager featureManager)
    {
        _logger = logger;
        _repositoryService = repositoryService;
        _registryService = registryService;
        _xdsConfig = xdsConfig;
        _featureManager = featureManager;
    }

    [Consumes("application/soap+xml", "application/xml", "multipart/related")]
    [Produces("application/soap+xml", ["application/xop+xml", "application/octet-stream"])]
    [HttpPost("RepositoryService")]
    public async Task<IActionResult> RepositoryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var xmlSerializer = new SoapXmlSerializer(XmlSettings.Soap);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;

        var responseEnvelope = new SoapEnvelope();
        var requestTimer = Stopwatch.StartNew();
        _logger.LogInformation($"Received request for action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");

        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti43Action:
                if (!await _featureManager.IsEnabledAsync("Iti43RetrieveDocumentSet")) return NotFound();

                var documentFetchResponse = await _repositoryService.RetrieveDocumentSet(soapEnvelope);
                if (documentFetchResponse.IsSuccess is false)
                {
                    break;
                }

                if (_xdsConfig.MultipartResponseForIti43 is true)
                {
                    var multipartContent = HttpRequestResponseExtensions.ConvertToMultipartResponse(documentFetchResponse.Value);

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = multipartContent
                    };

                    return new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = responseMessage.Content.ReadAsStringAsync().Result,
                        ContentType = Constants.MimeTypes.MultipartRelated
                    };
                }

                responseEnvelope = documentFetchResponse.Value;
                responseEnvelope.Header = new()
                {
                    Action = soapEnvelope.GetCorrespondingResponseAction(),
                    Security = null,
                    RelatesTo = soapEnvelope.Header.MessageId
                };
                break;


            case Constants.Xds.OperationContract.Iti41Action:
                if (!await _featureManager.IsEnabledAsync("Iti41ProvideAndRegisterDocumentSet")) return NotFound();

                if (soapEnvelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.Length == 0)
                {
                    responseEnvelope = SoapExtensions.CreateSoapFault("soapenv:Receiver", $"Unknown").Value;
                }

                var submittedDocumentsTooLarge = await _repositoryService.CheckIfDocumentsAreTooLarge(soapEnvelope);

                if (submittedDocumentsTooLarge.IsSuccess is false)
                {
                    responseEnvelope = submittedDocumentsTooLarge.Value;
                    break;
                }

                var iti42Message = _registryService.CopyIti41ToIti42Message(soapEnvelope);

                if (iti42Message.IsSuccess is false)
                {
                    responseEnvelope.Body ??= new();
                    responseEnvelope.Body.RegistryResponse = iti42Message.Value.Body.RegistryResponse;
                    responseEnvelope.Header = iti42Message.Value.Header;
                    break;
                }

                var repositoryDocumentExists = await _repositoryService.CheckIfDocumentExistsInRepository(soapEnvelope);

                if (repositoryDocumentExists.IsSuccess is false)
                {
                    responseEnvelope = repositoryDocumentExists.Value;
                    break;
                }

                var registerDocumentSetResponse = await _registryService.AppendToRegistryAsync(iti42Message.Value);

                if (registerDocumentSetResponse.IsSuccess is false)
                {
                    responseEnvelope = registerDocumentSetResponse.Value;
                    break;
                }

                var documentUploadResponse = await _repositoryService.UploadContentToRepository(soapEnvelope);

                if (documentUploadResponse.IsSuccess is false)
                {
                    responseEnvelope = documentUploadResponse.Value;
                    break;
                }

                responseEnvelope.Header = new()
                {
                    Action = soapEnvelope.GetCorrespondingResponseAction(),
                    Security = null,
                    RelatesTo = soapEnvelope.Header.MessageId
                };
                responseEnvelope.Body = documentUploadResponse.Value.Body;

                break;

            case Constants.Xds.OperationContract.Iti86Action:
                var removeDocumentsResponse = await _repositoryService.RemoveDocuments(soapEnvelope);
                if (removeDocumentsResponse.IsSuccess)
                {
                    responseEnvelope = removeDocumentsResponse.Value;
                }
                break;

            default:
                _logger.LogInformation($"Unknown action: {action} from {Request.HttpContext.Connection.RemoteIpAddress}");
                requestTimer.Stop();
                _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");
                return BadRequest(SoapExtensions.CreateSoapFault("soapenv:Reciever", detail: action, faultReason: $"The [action] cannot be processed at the receiver").Value);
        }

        requestTimer.Stop();
        _logger.LogInformation($"Completed action: {action} in {requestTimer.ElapsedMilliseconds} ms");

        return Ok(responseEnvelope);
    }

}
