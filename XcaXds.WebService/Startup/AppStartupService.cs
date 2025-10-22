using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Source;
using XcaXds.WebService.Services;

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

    public AppStartupService(
        ILogger<AppStartupService> logger,
        IHostEnvironment env,
        IConfiguration config,
        ApplicationConfig appConfig,
        MonitoringStatusService monitoringService,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper,
        PolicyRepositoryWrapper policyRepositoryWrapper
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
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var startupTime = DateTime.Now;
        _logger.LogInformation($"Startup Time (UTC): {startupTime.ToString("O")}");

        _monitoringService.StartupTime = startupTime;

        if (_env.IsProduction())
        {
            if (_appConfig.HomeCommunityId == "2.16.578.1.12.4.5.100.1.1")
            {
                _logger.LogCritical($"\n\n========  Fatal! Default HomeCommunityId in production =======\nDefault HomeCommunity Id {_appConfig.HomeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n");
                throw new InvalidOperationException("Default HomeCommunityId used in production environment.");
            }

            if (_appConfig.RepositoryUniqueId == "2.16.578.1.12.4.5.100.1.1.2")
            {
                _logger.LogCritical($"\n\n========  Fatal! Default RepositoryUniqueId in production =======\nUsing default Repository Unique Id {_appConfig.RepositoryUniqueId}!\nWhen deploying the application, please change this to an unique OID\n\n");
                throw new InvalidOperationException("Default HomeCommunityId used in production environment.");
            }
        }

        _logger.LogInformation("Starting XcaDocumentSource...");

        if (_appConfig.HomeCommunityId == "2.16.578.1.12.4.5.100.1.1")
        {
            _logger.LogWarning($"\n\n========  Warning! Default HomeCommunityId =======\nUsing default HomeCommunity Id {_appConfig.HomeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n");
        }

        if (_appConfig.RepositoryUniqueId == "2.16.578.1.12.4.5.100.1.1.2")
        {
            _logger.LogWarning($"\n\n========  Warning! Default RepositoryUniqueId =======\nUsing default Repository Unique Id {_appConfig.RepositoryUniqueId}!\nWhen deploying the application, please change this to an unique OID\n\n");
        }

        NormalizeAppconfigOidsWithRegistryRepositoryContent();

        AddDefaultAccessControlPolicies();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping XcaDocumentSource...");
        return Task.CompletedTask;
    }

    private void AddDefaultAccessControlPolicies()
    {
        var cz_deny_adhocquery_resourceid = new PolicyDto()
        {
            Id = "cz-deny-adhocquery-resourceid",
            AppliesTo = [Issuer.Helsenorge],
            Description = "Deny if the patient identifier in the resource-id SAML-attribute differs from the ITI-18 slot $XDSDocumentEntryPatientId (transformed to urn:no:nhn:xcads:adhocquery:patient-identifier)",
            Rules =
            [[
                new(Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier + ":code", CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code")
            ]],
            Actions = ["ReadDocumentList"],
            Effect = "Deny"
        };

        var cz_gp_deny_if_different_resourceid = new PolicyDto()
        {
            Id = "cz-gp-deny-if-different-resourceid",
            AppliesTo = [Issuer.Helsenorge, Issuer.HelseId],
            Description = "If the Citizen or healthcare personell is trying to access data for another patient, the correct acp value must be specified",
            Rules =
            [[
                new(Constants.Saml.Attribute.ProviderIdentifier + ":code",CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                new(Constants.Saml.Attribute.ProviderIdentifier + ":codeSystem", CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":codeSystem"),

                new(Constants.Xacml.CustomAttributes.DocumentEntryPatientIdentifier + ":code", CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                new(Constants.Xacml.CustomAttributes.DocumentEntryPatientIdentifier + ":codeSystem", CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":codeSystem"),

                new(Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier + ":code", CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":code"),
                new(Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier + ":codeSystem", CompareRule.NotEquals, Constants.Saml.Attribute.ResourceId20 + ":codeSystem"),

                new(Constants.Saml.Attribute.XuaAcp + ":code", Constants.Oid.Saml.Acp.NullValue)
            ]],
            Actions = ["ReadDocumentList", "ReadDocument"],
            Effect = "Deny"
        };

        var cz_readdocumentlist_documents = new PolicyDto()
        {
            Id = "cz-readdocumentlist-documents",
            AppliesTo = [Issuer.Helsenorge],
            Rules =
            [[
                new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),

                new(Constants.Saml.Attribute.Role + ":code", "13"),
                new(Constants.Saml.Attribute.Role + ":codeSystem", "1.0.14265.1")
            ]],
            Actions = ["ReadDocumentList", "ReadDocuments"],
            Effect = "Permit"
        };

        var gp_deny_certain_roles = new PolicyDto()
        {
            Id = "gp-deny2",
            AppliesTo = [Issuer.HelseId],
            Rules =
            [[
                new(Constants.Saml.Attribute.Role + ":code", "XX;VE;FB"),
                new(Constants.Saml.Attribute.Role + ":codeSystem", "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060")
            ]],
            Effect = "Deny"
        };

        var gp_readdocumentlist_readdocument_create = new PolicyDto()
        {
            Id = "gp-readdocumentlist-readdocument",
            AppliesTo = [Issuer.HelseId],
            Rules =
            [[
                new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),

                new(Constants.Saml.Attribute.Role + ":code", "LE;SP;PS"),
                new(Constants.Saml.Attribute.Role + ":codeSystem", "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060"),

                new(Constants.Saml.Attribute.PurposeOfUse + ":code", "TREAT"),
                new(Constants.Saml.Attribute.PurposeOfUse + ":codeSystem", "urn:oid:2.16.840.1.113883.1.11.20448;2.16.840.1.113883.1.11.20448")
            ]],
            Effect = "Permit"
        };

        _policyRepositoryWrapper.AddPolicy(cz_deny_adhocquery_resourceid);
        //_policyRepositoryWrapper.AddPolicy(cz_gp_deny_if_different_resourceid); // Remove because of incompatability with PIX
        _policyRepositoryWrapper.AddPolicy(cz_readdocumentlist_documents);
        _policyRepositoryWrapper.AddPolicy(gp_deny_certain_roles);
        _policyRepositoryWrapper.AddPolicy(gp_readdocumentlist_readdocument_create);
    }

    /// <summary>
    /// Normalize the metadata and repository ID with the configuration from appsettings.json
    /// This is useful if the OIDs for the application has changed, and the repositoryIds and HomecommunityIds are now different
    /// </summary>
    private void NormalizeAppconfigOidsWithRegistryRepositoryContent()
    {
        var registryContent = _registryWrapper.GetDocumentRegistryContentAsDtos();

        _logger.LogInformation("Normalizing registry entries");

        foreach (var registryObject in registryContent.OfType<DocumentEntryDto>())
        {
            registryObject.HomeCommunityId = _appConfig.HomeCommunityId;
            registryObject.RepositoryUniqueId = _appConfig.RepositoryUniqueId;
        }

        foreach (var registryObject in registryContent.OfType<SubmissionSetDto>())
        {
            registryObject.HomeCommunityId = _appConfig.HomeCommunityId;
            registryObject.SourceId = _appConfig.RepositoryUniqueId;
        }

        _registryWrapper.SetDocumentRegistryContentWithDtos(registryContent);

        var newIdSet = _repositoryWrapper.SetNewRepositoryOid(_appConfig.RepositoryUniqueId, out var oldId);

        if (newIdSet)
        {
            _logger.LogInformation($"New Repository Unique Id set: '{_appConfig.RepositoryUniqueId}' (old: '{oldId}')");
        }
    }
}
