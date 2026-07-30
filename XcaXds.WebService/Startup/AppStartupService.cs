using Microsoft.EntityFrameworkCore;
using NHN.OpenTelemetryExtensions;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.Source.Source;
using XcaXds.Source.Source.RegistryRepository.SqLite;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Startup;

public class AppStartupService : IHostedService
{
    private readonly ILogger<AppStartupService> _logger;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly MonitoringStatusService _monitoringService;
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;
    private readonly TerminologyService _terminologyService;
    private readonly TerminologyUpdaterService _terminologyUpdaterService;
    private readonly IDbContextFactory<SqliteRegistryDbContext> _sqliteRegistryContextFactory;

    public AppStartupService(
        ILogger<AppStartupService> logger,
        IHostEnvironment env,
        IConfiguration config,
        ApplicationConfig appConfig,
        MonitoringStatusService monitoringService,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper,
        PolicyRepositoryWrapper policyRepositoryWrapper,
        TerminologyService terminologyService,
        TerminologyUpdaterService terminologyUpdaterService,
        IDbContextFactory<SqliteRegistryDbContext> sqliteRegistryContextFactory
        )
    {
        _logger = logger;
        _env = env;
        _config = config;
        _appConfig = appConfig;
        _monitoringService = monitoringService;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _policyRepositoryWrapper = policyRepositoryWrapper;
        _terminologyService = terminologyService;
        _terminologyUpdaterService = terminologyUpdaterService;
        _sqliteRegistryContextFactory = sqliteRegistryContextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startupTime = DateTime.UtcNow;
        _logger.LogInformation("Starting XcaDocumentSource...");
        _logger.LogInformation("Startup Time (UTC): {startupTime}", startupTime.ToString("O"));

        _monitoringService.StartupTime = startupTime;

        if (_env.IsProduction())
        {
            if (_appConfig.HomeCommunityId == "2.16.578.1.12.4.5.100.1.1")
            {
                _logger.LogCritical("\n\n========  Fatal! Default HomeCommunityId in production =======\nDefault HomeCommunity Id {homeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n", _appConfig.HomeCommunityId);
                throw new InvalidOperationException("Default HomeCommunityId used in production environment.");
            }

            if (_appConfig.RepositoryUniqueId == "2.16.578.1.12.4.5.100.1.1.2")
            {
                _logger.LogCritical("\n\n========  Fatal! Default RepositoryUniqueId in production =======\nUsing default Repository Unique Id {repositoryUniqueId}!\nWhen deploying the application, please change this to an unique OID\n\n", _appConfig.RepositoryUniqueId);
                throw new InvalidOperationException("Default RepositoryUniqueId used in production environment.");
            }
        }


        if (_appConfig.HomeCommunityId == "2.16.578.1.12.4.5.100.1.1")
        {
            _logger.LogWarning("\n\n========  Warning! Default HomeCommunityId =======\nUsing default HomeCommunity Id {homeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n", _appConfig.HomeCommunityId);
        }

        if (_appConfig.RepositoryUniqueId == "2.16.578.1.12.4.5.100.1.1.2")
        {
            _logger.LogWarning("\n\n========  Warning! Default RepositoryUniqueId =======\nUsing default Repository Unique Id {repositoryUniqueId}!\nWhen deploying the application, please change this to an unique OID\n\n", _appConfig.RepositoryUniqueId);
        }

        NormalizeAppconfigOidsWithRegistryRepositoryContent();

        FindDudsInRepository();

        await AddDefaultAccessControlPolicies();
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping XcaDocumentSource...");
        return Task.CompletedTask;
    }

    private void FindDudsInRepository()
    {
        var registryContent = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentUniqueIds = registryContent.OfType<DocumentEntryDto>().Select(de => de.UniqueId);

        var duds = documentUniqueIds.Where(did => _repositoryWrapper.FileExistsInRepository(did) == false).ToList();

        foreach (var dud in duds)
        {
            _logger.LogWarning("Registry contains stale entry (No Repository metadata associated with it): {dud}. Removing...", dud);
            var registryObjectsForDud = _registryWrapper.GetRegistryItemAndRelated(dud)?.ToArray() ?? [];

            foreach (var registryObjectDud in registryObjectsForDud)
            {
                _registryWrapper.DeleteRegistryObjectFromRegistry(registryObjectDud);
            }
        }

        _logger.LogInformation("Removed {dudCount} stale entries from Registry", duds.Count);
    }

    private async Task AddDefaultAccessControlPolicies()
    {
        while (_terminologyUpdaterService.GetServiceState() != ServiceState.Ready &&
            _terminologyUpdaterService.GetServiceState() != ServiceState.Crashed)
        {
            _logger.LogInformation("Waiting for terminology service to initialize... (State: {state})", _terminologyUpdaterService.GetServiceState());
            await Task.Delay(1000);
        }

        var acpNullValue = _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Authentication.Acp, "NullValue")?.FirstOrDefault();

        var cz_deny_adhocquery_resourceid = new AbacPolicy()
        {
            Id = "DEFAULT_cz-deny-adhocquery-resourceid",
            AppliesTo = [AppliesTo.Helsenorge],
            Description = "Deny if the patient identifier in the resource-id SAML-attribute differs from the ITI-18 slot $XDSDocumentEntryPatientId (transformed to urn:no:nhn:xcads:adhocquery:patient-identifier)",
            Rules =
            [
                new(
                    new(Constants.Urn.Custom.AdhocQueryPatientIdentifier + ":code", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                    new(Constants.Saml.Attribute.XuaAcp + ":code", acpNullValue ?? "Unknown")
                )
            ],
            Actions = ["ReadDocumentList"],
            Effect = "Deny"
        };

        var cz_gp_deny_if_different_resourceid = new AbacPolicy()
        {
            Id = "DEFAULT_cz-gp-deny-if-different-resourceid",
            AppliesTo = [AppliesTo.Helsenorge, AppliesTo.HelseId],
            Description = "If the Citizen or healthcare personell is trying to access data for another patient, the correct acp value must be specified",
            Rules =
            [
                new(
                    new(Constants.Saml.Attribute.ProviderIdentifier + ":code", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                    new(Constants.Saml.Attribute.ProviderIdentifier + ":codeSystem", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":codeSystem"),

                    new(Constants.Urn.Custom.DocumentEntryPatientIdentifier + ":code", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                    new(Constants.Urn.Custom.DocumentEntryPatientIdentifier + ":codeSystem", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":codeSystem"),

                    new(Constants.Urn.Custom.AdhocQueryPatientIdentifier + ":code", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                    new(Constants.Urn.Custom.AdhocQueryPatientIdentifier + ":codeSystem", AttributeCompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":codeSystem"),

                    new(Constants.Saml.Attribute.XuaAcp + ":code", acpNullValue ?? "Unknown")
                )
            ],
            Actions = ["ReadDocumentList", "ReadDocuments"],
            Effect = "Deny"
        };

        var cz_readdocumentlist_documents = new AbacPolicy()
        {
            Id = "DEFAULT_cz-readdocumentlist-documents",
            AppliesTo = [AppliesTo.Helsenorge],
            Rules =
            [
                new(
                    new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),

                    new(Constants.Saml.Attribute.PurposeOfUse_Helsenorge + ":code", "13"),
                    new(Constants.Saml.Attribute.PurposeOfUse_Helsenorge + ":codeSystem", "1.0.14265.1")
                )
            ],
            Actions = ["ReadDocumentList", "ReadDocuments"],
            Effect = "Permit"
        };

        var gp_deny_certain_roles = new AbacPolicy()
        {
            Id = "DEFAULT_gp-deny2",
            AppliesTo = [AppliesTo.HelseId],
            Rules =
            [
                new(
                    new(Constants.Saml.Attribute.Role + ":code", "XX;VE;FB"),
                    new(Constants.Saml.Attribute.Role + ":codeSystem", "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060")
                )
            ],
            Effect = "Deny"
        };

        var gp_readdocumentlist_readdocument = new AbacPolicy()
        {
            Id = "DEFAULT_gp-readdocumentlist_readdocument",
            AppliesTo = [AppliesTo.HelseId],
            Rules =
            [
                new(
                    new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),

                    new(Constants.Saml.Attribute.Role + ":code", "LE;SP;PS"),
                    new(Constants.Saml.Attribute.Role + ":codeSystem", "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060"),

                    new(Constants.Saml.Attribute.PurposeOfUse + ":code", "TREAT;1;ETREAT;COC;BTG"),
                    new(Constants.Saml.Attribute.PurposeOfUse + ":codeSystem", "urn:oid:2.16.840.1.113883.1.11.20448;2.16.840.1.113883.1.11.20448;1.0.14265.1;urn:oid:1.0.14265.1")
                )
            ],
            //Actions = ["Create", "ReadDocumentList", "ReadDocuments", "Update", "Delete"],
            Actions = ["ReadDocumentList", "ReadDocuments"],
            Effect = "Permit"
        };

        var machine_create_update_documents = new AbacPolicy()
        {
            Id = "DEFAULT_machine_create_update_documents",
            AppliesTo = [AppliesTo.Machine],
            Rules =
            [
                new(
                    (AbacCondition)new(Constants.Saml.Attribute.EhelseScope, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments)
                )
            ],
            Actions = ["Create", "Update"],
            Effect = "Permit"
        };

        var machine_validate_documents = new AbacPolicy()
        {
            Id = "DEFAULT_machine_validate_documents",
            AppliesTo = [AppliesTo.Machine],
            Rules =
            [
                new(
                    (AbacCondition)new(Constants.Saml.Attribute.EhelseScope, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments)
                )
            ],
            Actions = ["Execute"],
            Effect = "Permit"
        };

        var machine_delete_documents = new AbacPolicy()
        {
            Id = "DEFAULT_machine_delete_documents",
            AppliesTo = [AppliesTo.Machine],
            Rules =
            [
                new(
                    (AbacCondition)new(Constants.Saml.Attribute.EhelseScope, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeDeleteDocument)
                )
            ],
            Actions = ["Delete"],
            Effect = "Permit"
        };

        var machine_read_document_status = new AbacPolicy()
        {
            Id = "DEFAULT_machine_read_document_status",
            AppliesTo = [AppliesTo.Machine],
            Rules =
            [
                new(
                    (AbacCondition)new(Constants.Saml.Attribute.EhelseScope, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments)
                )
            ],
            Actions = ["ReadDocumentList"],
            Effect = "Permit"
        };

        //_policyRepositoryWrapper.AddPolicy(cz_deny_adhocquery_resourceid); // Remove because of incompatability with PIX
        //_policyRepositoryWrapper.AddPolicy(cz_gp_deny_if_different_resourceid); // Remove because of incompatability with PIX
        _policyRepositoryWrapper.DeletePolicy(cz_readdocumentlist_documents.Id);
        _policyRepositoryWrapper.DeletePolicy(gp_deny_certain_roles.Id);
        _policyRepositoryWrapper.DeletePolicy(gp_readdocumentlist_readdocument.Id);
        _policyRepositoryWrapper.DeletePolicy(machine_create_update_documents.Id);
        _policyRepositoryWrapper.DeletePolicy(machine_delete_documents.Id);

        _policyRepositoryWrapper.AddPolicy(cz_readdocumentlist_documents);
        _policyRepositoryWrapper.AddPolicy(gp_deny_certain_roles);
        _policyRepositoryWrapper.AddPolicy(gp_readdocumentlist_readdocument);
        _policyRepositoryWrapper.AddPolicy(machine_create_update_documents);
        _policyRepositoryWrapper.AddPolicy(machine_delete_documents);
        _policyRepositoryWrapper.AddPolicy(machine_read_document_status);
        _policyRepositoryWrapper.AddPolicy(machine_validate_documents);
    }

    /// <summary>
    /// Normalize the metadata and repository ID with the configuration from appsettings.json
    /// This is useful if the OIDs for the application has changed, and the repositoryIds and HomecommunityIds are now different
    /// </summary>
    private void NormalizeAppconfigOidsWithRegistryRepositoryContent()
    {
        var registryContent = _registryWrapper.GetDocumentRegistryContentAsDtos().ToList();
        if (registryContent == null || registryContent.Count == 0) return;

        if (registryContent.OfType<DocumentEntryDto>().Any(de => de.HomeCommunityId == _appConfig.HomeCommunityId || de.RepositoryUniqueId == _appConfig.RepositoryUniqueId) ||
            registryContent.OfType<SubmissionSetDto>().Any(de => de.HomeCommunityId == _appConfig.HomeCommunityId))
        {
            return;
        }

        _logger.LogInformation("New OID Detected! Normalizing registry entries");

        foreach (var registryObject in registryContent)
        {
            switch (registryObject)
            {
                case DocumentEntryDto doc:
                    var oldHomeCommunityId = doc.HomeCommunityId;

                    doc.HomeCommunityId = _appConfig.HomeCommunityId;
                    doc.RepositoryUniqueId = _appConfig.RepositoryUniqueId;

                    if (string.IsNullOrWhiteSpace(doc.SourcePatientInfo?.PatientId?.System) ||
                        doc.SourcePatientInfo?.PatientId?.System == oldHomeCommunityId)
                    {
                        _logger.LogInformation("Fixing stale patient identifier System, new OID: {newOid}", _appConfig.HomeCommunityId);

                        doc.SourcePatientInfo!.PatientId!.System = _appConfig.HomeCommunityId;
                    }

                    break;

                case SubmissionSetDto sub:
                    sub.HomeCommunityId = _appConfig.HomeCommunityId;
                    sub.SourceId = _appConfig.RepositoryUniqueId;
                    break;
            }
        }

        var response = _registryWrapper.SetDocumentRegistryContentWithDtos(registryContent.ToList());

        if (response.IsSuccess)
        {
            _logger.LogInformation("Normalized {registryCount} registry entries", registryContent.Count);
        }
        else
        {
            _logger.LogError("Failed to normalize registry entries. Error: {errorMessage}", response.Message);
        }

        var newIdSet = _repositoryWrapper.SetNewRepositoryOid(_appConfig.RepositoryUniqueId, out var oldId);

        if (newIdSet)
        {
            _logger.LogInformation("New Repository Unique Id set: '{newRepositoryUniqueId}' (old: '{oldRepositoryUniqueId}')", _appConfig.RepositoryUniqueId, oldId);
        }
    }
}