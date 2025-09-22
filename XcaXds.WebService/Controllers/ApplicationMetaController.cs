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
public class ApplicationMetaController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;

    public ApplicationMetaController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
    }


    [Produces("application/json")]
    [HttpGet("about/registryobjects")]
    public async Task<IActionResult> CountRegistryObjects()
    {
        var objects = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntries = objects.OfType<DocumentEntryDto>().Count();
        var submissionSets = objects.OfType<SubmissionSetDto>().Count();
        var associations = objects.OfType<AssociationDto>().Count();
        
        return Ok(new{ documentEntries, submissionSets, associations });
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

        var files = jsonTestData.Documents.Select(file => Convert.FromBase64String(file));

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

                _repositoryWrapper.StoreDocument(generatedTestObject.UniqueId, randomFileAsByteArray, generatedTestObject.PatientId.Code);
            }
        }

        _registryWrapper.UpdateDocumentRegistryContentWithDtos(generatedTestRegistryObjects);

        return Ok("Metadata generated");
    }

    [Tags("_Purge registry and repository! ⚠️")]
    [HttpGet("get-nuke-key")]
    public async Task<IActionResult> GetNukeKey()
    {
        var datetime = DateTime.Now.ToString("ddMMyyhhMM");
        return Ok(new { nukeKey = datetime, superSecret = true });
    }

    [Tags("_Purge registry and repository! ⚠️")]
    [HttpGet("nuke")]
    public async Task<IActionResult> NukeRegistryRepository(string nukeKey)
    {
        var datetime = DateTime.Now.ToString("ddMMyyhhMM");
        if (datetime != nukeKey) return BadRequest("Invalid Nuke key, get nuke key from the 'get-nuke-key'-endpoint");

        var documentIds = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Select(dent => dent.Id).ToList();
        
        var amount = documentIds.Count;
        _logger.LogInformation($"Fetched {amount} for nuking");

        if (amount == 0)
        {
            return Ok("Nothing to nuke");
        }

        _registryWrapper.SetDocumentRegistryContentWithDtos(new List<RegistryObjectDto>());
        documentIds.ForEach(docid => _repositoryWrapper.DeleteSingleDocument(docid));

        return Ok($"Nuked {amount} entries!");
    }
}