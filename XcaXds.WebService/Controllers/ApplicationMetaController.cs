using Hl7.Fhir.Specification.Terminology;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using XcaInteropService.Commons.Enums;
using XcaInteropService.Commons.Models.Custom;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;
using XcaXds.Tests.Helpers;
using XcaXds.WebService.Extensions;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

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
    private readonly ImplementationInformerService _implementationInformerService;
    private readonly TerminologyService _terminologyService;
    private readonly DocumentListFiltererService _documentListFiltererService;
    private readonly BusinessRulesDescriptorService _businessRulesDescriptorService;
    private readonly IVariantFeatureManager _featureManager;
    private readonly SamlValidatorService _samlValidatorService;

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
        ImplementationInformerService implementationInformerService,
        TerminologyService terminologyService,
        IVariantFeatureManager featureManager,
        DocumentListFiltererService documentListFiltererService,
        BusinessRulesDescriptorService businessRulesDescriptorService,
        SamlValidatorService samlValidatorService)
    {
        _logger = logger;
        _appConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _healthCheckService = healthCheckService;
        _monitoringService = monitoringService;
        _requestThrottlingService = requestThrottlingService;
        _applicationMetaService = applicationMetaService;
        _implementationInformerService = implementationInformerService;
        _featureManager = featureManager;
        _terminologyService = terminologyService;
        _documentListFiltererService = documentListFiltererService;
        _businessRulesDescriptorService = businessRulesDescriptorService;
        _samlValidatorService = samlValidatorService;
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
                Action = g.Key,
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
            _monitoringService.LastRequest,
            _monitoringService.LastAtnaLogExported,

            RegistryRepository = regRepoReport
        };

        var healthCheckJson = JsonSerializer.Serialize(healthCheck, Constants.JsonDefaultOptions.DefaultSettings);
        if (!regRepoReport.RegistryOk || !regRepoReport.RepositoryOk)
        {
            return StatusCode(500, healthCheckJson);
        }

        return StatusCode(200, healthCheckJson);
    }

    [HttpGet("implementations")]
    public async Task<IActionResult> Implementations()
    {
        return Ok(_implementationInformerService.GetImplementations());
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
        var patientNin = _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Other.PersonAssigningAuthorities, "NIN")?.FirstOrDefault();

        var config = new DomainConfig()
        {
            Enabled = true,
            Async = false,
            FriendlyName = _appConfig.HostName?.Split("-xcadocumentsource").FirstOrDefault(),
            HomeCommunityId = _appConfig.HomeCommunityId,
            PatientResolverType = PatientResolverType.IDENTITY,
            Return = DomainReturn.DocumentList,
            PatientAssigningAuthority = patientNin,
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

    [Produces("application/json")]
    [HttpGet("about/samlvalidationparameters")]
    public async Task<IActionResult> GetSamlValidationParameters()
    {
        await _samlValidatorService.CreateSamlValidator();
        return Ok(_samlValidatorService.GetSamlValidationParameters());
    }

    [RequiresApiKey]
    [HttpGet("generate-random-test-data")]
    public async Task<IActionResult> GenerateRandomTestData([FromQuery] int entriesToGenerate, [FromQuery] string? patientIdentifier, [FromQuery] bool association = true, [FromQuery] bool submissionSet = true, [FromQuery] bool documentEntry = true, [FromQuery] bool document = true)
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();
        var response = new RestfulApiResponse();

        var patientIdentifierCx = Hl7Object.Parse<CX>(patientIdentifier);

        var existingPatientPidObject = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>()
            .Select(de => de.SourcePatientInfo).DistinctBy(pid => new { pid?.PatientId?.Id, pid?.PatientId?.System })
            .OfType<SourcePatientInfo>()
            .FirstOrDefault(pid => pid.PatientId?.Id == patientIdentifierCx?.IdNumber && pid.PatientId?.System == patientIdentifierCx?.AssigningAuthority?.UniversalId)?
            .AsHl7Pid().Serialize();

        var generatedRegistryObjects = TestHelpers.GenerateComprehensiveRegistryMetadata(entriesToGenerate, existingPatientPidObject ?? patientIdentifierCx?.Serialize() ?? patientIdentifier, false);
        _logger.LogInformation("Generated {count} registry objects", generatedRegistryObjects.Count());

        generatedRegistryObjects = generatedRegistryObjects.Select(ro =>
        new DocumentReferenceDto()
        {
            Association = association ? ro.Association : null,
            DocumentEntry = documentEntry ? ro.DocumentEntry : null,
            Document = document ? ro.Document : null,
            SubmissionSet = submissionSet ? ro.SubmissionSet : null
        }).ToList();

        return Ok(RegistryJsonSerializer.Serialize(generatedRegistryObjects));
    }

    [RequiresApiKey]
    [HttpPost("generate-test-data")]
    public async Task<IActionResult> GenerateTestData([FromBody] JsonElement resourceJson,
        [FromQuery] int entriesToGenerate, [FromQuery] string? patientIdentifier)
    {
        if (!await _featureManager.IsEnabledAsync("ApplicationMetaEndpoints_Debug")) return NotFound();

        var response = new RestfulApiResponse();

        var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(resourceJson.GetRawText());
        if (jsonTestData == null) return BadRequest("No content provided");

        var generatedRegistryObjects = RegistryMetadataGenerator.GenerateRandomizedTestData(_appConfig.HomeCommunityId,
            _appConfig.RepositoryUniqueId, jsonTestData, entriesToGenerate, patientIdentifier);

        _logger.LogInformation("Generated {count} registry objects", generatedRegistryObjects.Count());
        _logger.LogInformation("Updating registry with generated objects...");

        var registryObjects = generatedRegistryObjects.AsRegistryObjectDtos().ToList();

        var updateResponse = _registryWrapper.AddDocumentReferenceDtosToDocumentRegistry(registryObjects);

        foreach (var document in generatedRegistryObjects.Select(d => d.Document).OfType<DocumentDto>())
        {
            var documentEntry = registryObjects.OfType<DocumentEntryDto>().FirstOrDefault(ro => ro.UniqueId == document.DocumentId);
            var pidCx = documentEntry?.SourcePatientInfo?.PatientId is { } pid ? new CX()
            {
                IdNumber = pid.Id,
                AssigningAuthority = new(pid.System)
            } : null;
            var documentResponse = _repositoryWrapper.StoreDocument(document.DocumentId!, document.Data!, pidCx!.Serialize()!);
        }

        if (!updateResponse.IsSuccess)
        {
            var documentReference = generatedRegistryObjects.Select(d => d.Document).OfType<DocumentDto>().FirstOrDefault();

            return BadRequest(response.AddError("UpdateError", $"Error while generating test data {updateResponse.Message}"));
        }

        _logger.LogInformation("Metadata generated");

        return Ok(response.SetMessage("Metadata generated"));
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
        return Ok(_documentListFiltererService.BusinessLogicRules.Select(br => br.Key));
    }

    [Produces("text/plain")]
    [HttpGet("business-logic")]
    public async Task<IActionResult> GetBusinessLogicRules(bool plainText)
    {
        return Ok(
            plainText ? _businessRulesDescriptorService.WriteBusinessRulesPlainText() : _businessRulesDescriptorService.WriteBusinessRulesJsonFormatted());
    }

    [Produces("text/plain")]
    [HttpGet("business-logic-obfuscation")]
    public async Task<IActionResult> GetObfuscationRules()
    {
        return Ok(_businessRulesDescriptorService.WriteEntriesToObfuscateJsonFormatted());
    }
}