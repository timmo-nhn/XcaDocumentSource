using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime;
using System.Security.Cryptography;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class ApplicationMetaController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly HealthCheckService _healthCheckService;
    private readonly MonitoringStatusService _monitoringService;

    public ApplicationMetaController(ILogger<XdsRegistryController> logger, ApplicationConfig xdsConfig, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper, HealthCheckService healthCheckService, MonitoringStatusService monitoringService)
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _healthCheckService = healthCheckService;
        _monitoringService = monitoringService;
    }

    [HttpGet("health-check")]
    public async Task<IActionResult> HealthCheck()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();

        var entries = healthReport.Entries;
        var status = healthReport.Status.ToString();

        var stats = _monitoringService.ResponseTimes?.Items
            .GroupBy(itm => itm.Key)
            .Select(g => new
            {
                Key = g.Key,
                Min = g.Min(x => x.Value),
                Max = g.Max(x => x.Value),
                Avg = g.Average(x => x.Value),
                Amount = g.Count()
            })
            .ToList();

        var healthCheck = new
        {
            HealthReport = healthReport,
            stats
        };

        var healthCheckJson = JsonSerializer.Serialize(healthCheck, Constants.JsonDefaultOptions.DefaultSettings);
        return Content(healthCheckJson);
    }

    [Produces("application/json")]
    [HttpGet("about/registryobjects")]
    public async Task<IActionResult> CountRegistryObjects()
    {
        var objects = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntries = objects.OfType<DocumentEntryDto>().Count();
        var submissionSets = objects.OfType<SubmissionSetDto>().Count();
        var associations = objects.OfType<AssociationDto>().Count();

        return Ok(new { documentEntries, submissionSets, associations });
    }


    [Produces("application/json")]
    [HttpGet("about/config")]
    public async Task<IActionResult> GetXdsConfig()
    {
        return Ok(_xdsConfig);
    }


    [HttpPost("generate-test-data")]
    public async Task<IActionResult> GenerateTestData([FromBody] JsonElement resourceJson, [FromQuery] int entriesToGenerate, [FromQuery] string? patientIdentifier)
    {
        var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(resourceJson.GetRawText());
        if (jsonTestData == null) return BadRequest("No content provided");

        jsonTestData.PossibleSubmissionSetValues.Authors ??= jsonTestData.PossibleDocumentEntryValues.Authors;

        entriesToGenerate = entriesToGenerate == 0 ? 10 : entriesToGenerate;

        var sourcePatientInfoForPatient = jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos.FirstOrDefault(spi => spi?.PatientId?.Id == patientIdentifier);
            
        if (sourcePatientInfoForPatient != null)
        {
            jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos = [sourcePatientInfoForPatient];
        }
        var generatedTestRegistryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, entriesToGenerate);

        var files = jsonTestData.Documents.Select(file => Convert.FromBase64String(file));

        foreach (var generatedTestObject in generatedTestRegistryObjects.OfType<DocumentEntryDto>())
        {
            var randomFileAsByteArray = files.ElementAt(Random.Shared.Next(files.Count()));

            if (generatedTestObject?.SourcePatientInfo?.PatientId?.Id != null && generatedTestObject.Id != null && randomFileAsByteArray != null)
            {
                generatedTestObject.Title = "XcaDS - " + generatedTestObject.Title;
                generatedTestObject.Size = randomFileAsByteArray.Length.ToString();
                using (var md5 = MD5.Create())
                {
                    generatedTestObject.Hash = BitConverter.ToString(md5.ComputeHash(randomFileAsByteArray)).Replace("-", "");
                }

                generatedTestObject.RepositoryUniqueId = _xdsConfig.RepositoryUniqueId;
                generatedTestObject.HomeCommunityId = _xdsConfig.HomeCommunityId;

                _repositoryWrapper.StoreDocument(generatedTestObject.UniqueId, randomFileAsByteArray, generatedTestObject.SourcePatientInfo.PatientId.Id);
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