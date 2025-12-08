using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
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
    private readonly RequestThrottlingService _requestThrottlingService;

    private static readonly ActivitySource ActivitySource = new("nhn.xcads.healthz");

    public ApplicationMetaController(
        ILogger<XdsRegistryController> logger, 
        ApplicationConfig xdsConfig, 
        RegistryWrapper registryWrapper, 
        RepositoryWrapper repositoryWrapper, 
        HealthCheckService healthCheckService, 
        MonitoringStatusService monitoringService,
        RequestThrottlingService requestThrottlingService
        )
    {
        _logger = logger;
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _healthCheckService = healthCheckService;
        _monitoringService = monitoringService;
        _requestThrottlingService = requestThrottlingService;
    }

    [HttpGet("health-check")]
    public async Task<IActionResult> HealthCheck()
    {
        using var activity = ActivitySource.StartActivity("healthz");

        var healthReport = await _healthCheckService.CheckHealthAsync();

        var entries = healthReport.Entries;
        var status = healthReport.Status.ToString();

        var uptimeInSeconds = double.Round((DateTimeOffset.Now - _monitoringService.StartupTime).TotalSeconds);

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
            stats,
            uptimeInSeconds,
            _monitoringService.StartupTime
        };

        var healthCheckJson = JsonSerializer.Serialize(healthCheck, Constants.JsonDefaultOptions.DefaultSettings);
        return Content(healthCheckJson);
    }

    [HttpGet("set-get-throttle-time")]
    public IActionResult SetOrGetThrottleTime(int? throttleTimeMillis = null)
    {
        var response = new RestfulApiResponse();

        if (throttleTimeMillis == null)
        {
            var responseMessage = $"Fake throttle time: {_requestThrottlingService.GetThrottleTime()} ms";

            _logger.LogInformation(responseMessage);
            response.SetMessage(responseMessage);
        }
        else
        {
            _requestThrottlingService.SetThrottleTime(throttleTimeMillis ?? 0);

            var responseMessage = $"Fake throttle time set to {throttleTimeMillis} ms";

            _logger.LogInformation(responseMessage);
            response.SetMessage(responseMessage);
        }

        return Ok(response);
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

        var generatedTestRegistryObjects = RegistryMetadataGeneratorService.GenerateRandomizedTestData(_xdsConfig, jsonTestData, _repositoryWrapper, entriesToGenerate, patientIdentifier);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(generatedTestRegistryObjects);

        return Ok("Metadata generated");
    }

    [HttpGet("debug-patient-identifiers")]
    public async Task<IActionResult> PatientIdentifiers()
    {
        var patientIdentifiers = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Select(de => de.SourcePatientInfo).DistinctBy(pid => new { pid?.PatientId?.Id, pid?.PatientId?.System }).ToList();
        return Ok(patientIdentifiers);
    }


    [Tags("_Purge registry and repository! ⚠️")]
    [HttpGet("get-nuke-key")]
    public async Task<IActionResult> GetNukeKey()
    {
        var datetime = DateTime.Now.ToString("ddMMyyhhMM");
        return Ok(new { nukeKey = datetime, superSecret = true });
    }

    [Tags("_Purge registry and repository! ⚠️")]
    [HttpDelete("nuke")]
    public async Task<IActionResult> NukeRegistryRepository(string nukeKey)
    {
        var datetime = DateTime.Now.ToString("ddMMyyhhMM");
        if (datetime != nukeKey) return BadRequest("Invalid Nuke key, get nuke key from the 'get-nuke-key'-endpoint");

        var documentIds = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Select(dent => dent.UniqueId).ToList();

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