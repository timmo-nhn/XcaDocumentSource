using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Text.Json;
using XcaInteropService.Commons.Enums;
using XcaInteropService.Commons.Models.Custom;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Controllers;

[ApiController]
[Route("api")]
public class ApplicationMetaController : ControllerBase
{
    private readonly ILogger<XdsRegistryController> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly SourceHealthCheckService _healthCheckService;
    private readonly MonitoringStatusService _monitoringService;
    private readonly RequestThrottlingService _requestThrottlingService;
    private readonly ApplicationMetaService _applicationMetaService;
    private readonly IVariantFeatureManager _featureManager;

    private static readonly ActivitySource ActivitySource = new("nhn.xcads.healthz");

    public ApplicationMetaController(
        ILogger<XdsRegistryController> logger,
        ApplicationConfig xdsConfig,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper,
        SourceHealthCheckService healthCheckService,
        MonitoringStatusService monitoringService,
        RequestThrottlingService requestThrottlingService,
        ApplicationMetaService applicationMetaService,
        IVariantFeatureManager featureManager
    )
    {
        _logger = logger;
        _appConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _healthCheckService = healthCheckService;
        _monitoringService = monitoringService;
        _requestThrottlingService = requestThrottlingService;
        _applicationMetaService = applicationMetaService;
        _featureManager = featureManager;
    }

    [HttpGet("health-check")]
    public async Task<IActionResult> HealthCheck()
    {
        using var activity = ActivitySource.StartActivity("healthz");

        var healthReport = await _healthCheckService.CheckHealthAsync();
        var regRepoReport = await _healthCheckService.GetRegistryRepositoryStatus();

        var uptimeInSeconds = double.Round((DateTimeOffset.Now - _monitoringService.StartupTime).TotalSeconds);

        var usageStatistics = _monitoringService.ResponseTimes?.Items
            .GroupBy(itm => itm.Key)
            .Select(g => new
            {
                Key = g.Key,
                Min = g.Min(x => x.Value),
                Max = g.Max(x => x.Value),
                Avg = g.Average(x => x.Value),
                Amount = g.Count(),
            })
            .ToList();

        var healthCheck = new
        {
            HealthReport = healthReport,
            usageStatistics,
            uptimeInSeconds,
            _monitoringService.StartupTime,
            RegistryRepository = regRepoReport
        };

        var healthCheckJson = JsonSerializer.Serialize(healthCheck, Constants.JsonDefaultOptions.DefaultSettings);
        if (regRepoReport.RegistryOk || regRepoReport.RepositoryOk)
        {
            return StatusCode(500, healthCheckJson);
        }

        return StatusCode(200, healthCheckJson);
    }

    [RequiresApiKey]
    [HttpGet("set-get-throttle-time")]
    public async Task<IActionResult> SetOrGetThrottleTime(int? throttleTimeMillis = null, int? throttleDurationSeconds = 0)
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();

        var response = new RestfulApiResponse();

        if (throttleTimeMillis == null)
        {
            var responseMessage = $"Fake throttle time: {_requestThrottlingService.GetThrottleTime()} ms";

            _logger.LogInformation(responseMessage);
            response.SetMessage(responseMessage);
        }
        else
        {
            _requestThrottlingService.SetThrottleTime(throttleTimeMillis ?? 0, throttleDurationSeconds ?? 30);

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
    [HttpGet("about/domain-config")]
    public async Task<IActionResult> GetDomainConfig()
    {
        var config = new DomainConfig()
        {
            Enabled = true,
            Async = false,
            FriendlyName = _appConfig.HostName.Split("-xcadocumentsource").FirstOrDefault(),
            HomeCommunityId = _appConfig.HomeCommunityId,
            PatientResolverType = PatientResolverType.IDENTITY,
            Return = DomainReturn.DocumentList,
            PatientAssigningAuthority = Constants.Oid.Fnr,
            QueryUrl = GetFullQueryUrl(),
        };

        return Ok(config);
    }

    private string GetFullQueryUrl()
    {
        var url = Request.GetDisplayUrl().Split(Request.Path.Value).FirstOrDefault()?.Replace("http://", "https://");
        _logger.LogInformation("Base URL for query endpoint: {url}", url);
        return url + "/XCA/services/RespondingGatewayService";
    }

    [Produces("application/json")]
    [HttpGet("about/config")]
    public async Task<IActionResult> GetXdsConfig()
    {
        return Ok(_appConfig);
    }

    [RequiresApiKey]
    [HttpPost("generate-test-data")]
    public async Task<IActionResult> GenerateTestData([FromBody] JsonElement resourceJson,
        [FromQuery] int entriesToGenerate, [FromQuery] string? patientIdentifier)
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();

        var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(resourceJson.GetRawText());
        if (jsonTestData == null) return BadRequest("No content provided");

        var generatedRegistryObjects = RegistryMetadataGenerator.GenerateRandomizedTestData(_appConfig.HomeCommunityId,
            _appConfig.RepositoryUniqueId, jsonTestData, entriesToGenerate, patientIdentifier);

        _logger.LogInformation("Generated {count} registry objects", generatedRegistryObjects.Count());
        _logger.LogInformation("Updating registry with generated objects...");

        _registryWrapper.UpdateDocumentRegistryContentWithDtos(generatedRegistryObjects.AsRegistryObjectDtos()
            .ToList());

        return Ok("Metadata generated");
    }

    [RequiresApiKey]
    [HttpGet("debug-patient-identifiers")]
    public async Task<IActionResult> PatientIdentifiers()
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();

        var patientIdentifiers = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>()
            .Select(de => de.SourcePatientInfo).DistinctBy(pid => new { pid?.PatientId?.Id, pid?.PatientId?.System })
            .ToList();
        return Ok(patientIdentifiers);
    }

    [RequiresApiKey]
    [Tags("_Purge registry and repository! ⚠️")]
    [HttpGet("get-nuke-key")]
    public async Task<IActionResult> GetNukeKey()
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();

        var dateTime = _applicationMetaService.GetNukeKeyForRegistryRepository();
        return Ok(new { nukeKey = dateTime, superSecret = true });
    }

    [RequiresApiKey]
    [Tags("_Purge registry and repository! ⚠️")]
    [HttpDelete("nuke")]
    public async Task<IActionResult> NukeRegistryRepository(string nukeKey)
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();

        var apiResponse = _applicationMetaService.NukeRegistryRepository(nukeKey);
        return Ok(apiResponse);
    }

    [Produces("application/json")]
    [HttpGet("business-logic-names")]
    public async Task<IActionResult> GetBusinessLogicNames()
    {
        return Ok(BusinessLogicFilterer.BusinessLogicRules.Select(br => br.Name));
    }

    [Produces("text/plain")]
    [HttpGet("business-logic")]
    public async Task<IActionResult> GetBusinessLogicRules(bool plainText)
    {
        return Ok(
            plainText ? BusinessRulesDescriptor.BusinessRulesPlainText : BusinessRulesDescriptor.BusinessRulesJson);
    }

    [Produces("text/plain")]
    [HttpGet("business-logic-obfuscation")]
    public async Task<IActionResult> GetObfuscationRules()
    {
        return Ok(BusinessRulesDescriptor.EntriesToObfuscateJson);
    }
}