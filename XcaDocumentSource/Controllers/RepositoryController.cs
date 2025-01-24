using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;
using XcaDocumentSource.Models.Soap;
using XcaDocumentSource.Models.Soap.XdsTypes;
using XcaDocumentSource.Xca;

namespace XcaDocumentSource.Controllers;

[ApiController]
[Route("Repository/services")]
public class RepositoryController : ControllerBase
{
    private readonly ILogger<RegistryController> _logger;
    private readonly HttpClient _httpClient;
    private readonly XcaGateway _xcaGateway;

    public RepositoryController(ILogger<RegistryController> logger, HttpClient httpClient, XcaGateway xcaGateway)
    {
        _logger = logger;
        _httpClient = httpClient;
        _xcaGateway = xcaGateway;
    }

    [Consumes("application/soap+xml")]
    [HttpPost("RepositoryService")]
    public async Task<IActionResult> RepositoryService([FromBody] SoapEnvelope soapEnvelope)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var action = soapEnvelope.Header.Action;
        switch (soapEnvelope.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti43Action:
                // Hent dokument!!!
                break;
            case Constants.Xds.OperationContract.Iti41Action:
                var registryObjectList = soapEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;
                var associations = soapEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<AssociationType>().ToArray();
                var registryPackages = soapEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<RegistryPackageType>().ToArray();
                var extrinsicObjects = soapEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().ToArray();
                var documents = soapEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.Document;
                foreach (var extrinsicObject in extrinsicObjects)
                {

                    var extrinsicObjectsDocument = documents?.Where(doc => doc.Id == extrinsicObject.Id).FirstOrDefault();
                    var patientId = extrinsicObject.Slot.FirstOrDefault(s => s.Name == "sourcePatientId")?.ValueList.Value.FirstOrDefault();
                    var patientIdPart = patientId.Substring(0, patientId.IndexOf("^^^"));
                    var documentTitle = extrinsicObject.Name.LocalizedString.FirstOrDefault()?.Value;

                    string baseDirectory = AppContext.BaseDirectory;
                    string repositoryPath = Path.Combine(baseDirectory, "..", "..", "..", "Repository", patientIdPart);
                    var documentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (!Directory.Exists(repositoryPath))
                    {
                        Directory.CreateDirectory(repositoryPath);
                    }

                    // Create a file in the Repository folder
                    string filePath = Path.Combine(repositoryPath, documentTitle);
                    System.IO.File.WriteAllBytes(filePath, extrinsicObjectsDocument.Value);
                    Console.WriteLine($"File successfully written to: {filePath}");

                }
                var B = ";;;";
                break;


            default:
                if (action != null)
                {

                    return Ok(soapEnvelope.CreateSoapFault("UnknownAction", $"Unknown action {action}"));
                }
                else
                {
                    return Ok(soapEnvelope.CreateSoapFault("MissingAction", $"Missing action {action}".Trim(), "fix action m8"));
                }
        }

        return Ok($"Response from Responding Gateway: ");
    }
}
