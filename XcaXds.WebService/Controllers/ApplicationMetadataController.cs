using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class ApplicationMetadataController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;

    public ApplicationMetadataController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
    }


    [Produces("application/json")]
    [HttpGet("about/config")]
    public async Task<IActionResult> GetXdsConfig()
    {
        return Ok(_xdsConfig);
    }

    [HttpPost("generate-test-data")]
    public async Task<IActionResult> GenerateTestData([FromBody] JsonElement resourceJson, [FromQuery] int entriesToGenerate)
    {
        var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(resourceJson.GetRawText());
        if (jsonTestData == null) return BadRequest("No content provided");

        jsonTestData.PossibleSubmissionSetValues.Authors ??= jsonTestData.PossibleDocumentEntryValues.Authors;

        entriesToGenerate = entriesToGenerate == 0 ? 10 : entriesToGenerate;

        var generatedTestRegistryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, entriesToGenerate);

        var files = jsonTestData.Documents.Select(file => Encoding.UTF8.GetBytes(file));

        foreach (var generatedTestObject in generatedTestRegistryObjects.OfType<DocumentEntryDto>())
        {
            var randomFileAsByteArray = files.ElementAt(Random.Shared.Next(files.Count()));

            if (generatedTestObject?.PatientId?.Code != null && generatedTestObject.Id != null && randomFileAsByteArray != null)
            {
                generatedTestObject.Title = "XcaDS - " + generatedTestObject.Title;
                generatedTestObject.Size = randomFileAsByteArray.Length.ToString();
                using (var md5 = MD5.Create())
                {
                    generatedTestObject.Hash = BitConverter.ToString(md5.ComputeHash(randomFileAsByteArray)).Replace("-", "");
                }

                generatedTestObject.RepositoryUniqueId = _xdsConfig.RepositoryUniqueId;
                generatedTestObject.HomeCommunityId = _xdsConfig.HomeCommunityId;

                _repositoryWrapper.StoreDocument(generatedTestObject.Id, randomFileAsByteArray, generatedTestObject.PatientId.Code);
            }
        }

        _registryWrapper.UpdateDocumentRegistryContentWithDtos(generatedTestRegistryObjects);

        return Ok("Metadata generated");
    }
}