using Microsoft.AspNetCore.Mvc;
using System.Net;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Xca;
using XcaXds.Source.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("XCA/services")]
public class RespondingGatewayController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;
    private readonly XdsConfig _xdsConfig;
    private readonly RepositoryService _repositoryService;

    public RespondingGatewayController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway, XdsConfig xdsConfig, RepositoryService repositoryService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
        _xdsConfig = xdsConfig;
        _repositoryService = repositoryService;
    }

    [Consumes("application/soap+xml")]
    [Produces("application/soap+xml", "application/xop+xml", "application/octet-stream", "multipart/related")]
    [HttpPost("RespondingGatewayService")]
    public async Task<IActionResult> CrossGatewayQuery([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action.Trim();
        switch (action)
        {
            case Constants.Xds.OperationContract.Iti38Action:
                // Only change from ITI-38 to ITI-18 is the action in the header
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti18Action);
                var iti38Response = await _xcaGateway.RegistryStoredQuery(soapEnvelope, baseUrl + "/Registry/services/RegistryService");
                return Ok(iti38Response);

            case Constants.Xds.OperationContract.Iti39Action:
                soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti43Action);
                var iti39Response = await _xcaGateway.RetrieveDocumentSet(soapEnvelope, baseUrl + "/Repository/services/RepositoryService");
                if (iti39Response.IsSuccess is false)
                {
                    return Ok(iti39Response.Value);
                }

                if (_xdsConfig.MultipartResponseForIti43 is true)
                {
                    var multipartContent = _repositoryService.ConvertToMultipartResponse(iti39Response.Value);

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = multipartContent
                    };

                    return new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = responseMessage.Content.ReadAsStringAsync().Result, // Read content as string
                        ContentType = Constants.MimeTypes.MultipartRelated
                    };
                }
                return Ok(iti39Response.Value);
        }

        if (action != null)
        {
            _logger.LogError($"Unknown action {action}");
            return Ok(SoapExtensions.CreateSoapFault("UnknownAction", $"Unknown action {action}"));
        }
        else
        {
            _logger.LogError($"Missing action {action}");
            return Ok(SoapExtensions.CreateSoapFault("MissingAction", $"Missing action {action}"));
        }
    }
}