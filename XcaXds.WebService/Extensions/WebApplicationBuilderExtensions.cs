using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions.NinParsers;
using XcaXds.Commons.Extensions.No;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Interfaces.Statistics;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.Commons.Models.PolicyEnforcementPoint.DenyStrategies;
using XcaXds.Source.Source;
using XcaXds.Source.Source.PolicyRepository;
using XcaXds.Source.Source.PolicyRepository.FileBased;
using XcaXds.Source.Source.RegistryRepository.FileBased;
using XcaXds.Source.Source.RegistryRepository.PostGreSql;
using XcaXds.Source.Source.RegistryRepository.SqLite;
using XcaXds.Terminology.Services;
using XcaXds.Terminology.Sources;
using XcaXds.Terminology.TerminologySources;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.AtnaAuditLogging;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;
using XcaXds.WebService.Services.Fhir;
using XcaXds.WebService.Services.PolicyEnforcementPoint.DenyBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint.DenyStrategies;
using XcaXds.WebService.Services.PolicyEnforcementPoint.InputBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy.RequestMappers;
using XcaXds.WebService.Services.Statistics;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void RegisterAuditLoggingServices(this WebApplicationBuilder builder)
    {
        // Atna log builder and strategies
        builder.Services.AddScoped<AtnaLogBuilder>();
        builder.Services.AddScoped<IAtnaLogStrategy, SoapEnvelopeAtnaLogStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirPatchDocumentAtnaLogStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirDeleteDocumentsAtnaLogStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirValidateBundleAtnaLogStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirProvideBundleAtnaLogStrategy>();

        builder.Services.AddSingleton<IAtnaLogQueue, AtnaLogQueue>();
        builder.Services.AddScoped<AtnaLogGeneratorService>();
        builder.Services.AddSingleton<AtnaLogEnricherService>();
    }

    public static void RegisterBusinessLogicServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<BusinessLogicFiltersRegistry>();
        builder.Services.AddSingleton<BusinessLogicMapperService>();
    }

    public static void RegisterMetaAndStatusServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ApplicationMetaService>();
        builder.Services.AddSingleton<MonitoringStatusService>();
        builder.Services.AddSingleton<RequestThrottlingService>();
        builder.Services.AddSingleton<SourceHealthCheckService>();
        builder.Services.AddSingleton<BusinessRulesDescriptorService>();
    }

    public static void RegisterFhirServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<FhirService>();
        builder.Services.AddSingleton<FhirResourceValidatorService>();
        builder.Services.AddSingleton<XdsOnFhirTransformerService>();
        builder.Services.AddSingleton<FhirToXdsTransformerService>();
    }

    public static void RegisterNinServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<INinParser, NorwegianNinParser>();
        builder.Services.AddSingleton<NinParserFactory>();
    }

    public static void RegisterPolicyEnforcementPointServices(this WebApplicationBuilder builder)
    {
        // Policy input builder and strategies
        builder.Services.AddScoped<PolicyInputBuilder>();
        builder.Services.AddScoped<IPolicyInputStrategy, FhirJsonPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, SoapSamlXmlPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, JsonPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, GenericPolicyInputStrategy>();

        // Policy deny response builder and strategies
        builder.Services.AddScoped<PolicyDenyResponseBuilder>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, SoapDenyResponseStrategy>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, FhirDenyResponseStrategy>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, JsonDenyResponseStrategy>();

        // PDP services
        builder.Services.AddSingleton<PolicyRepositoryService>();
        builder.Services.AddSingleton<PolicyRepositoryWrapper>();
        builder.Services.AddSingleton<IPolicyRepository, FileBasedPolicyRepository>();

        builder.Services.AddSingleton<PolicyDecisionPointService>();

        // Validation and certificate services
        builder.Services.AddSingleton<SamlValidatorService>();
        builder.Services.AddSingleton<SigningCertificateFetcherService>();

        builder.Services.AddSingleton<SamlPolicyRequestMapper>();
        builder.Services.AddSingleton<JsonWebTokenPolicyRequestMapper>();
    }

    public static void RegisterStatisticsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<StatisticsTransformerService>();
        builder.Services.AddSingleton<IStatisticsQueue, StatisticsQueue>();
    }

    public static void RegisterTerminologyServices(this WebApplicationBuilder builder)
    {
        // Terminology sources
        builder.Services.AddSingleton<HttpTerminologySource>();
        builder.Services.AddSingleton<FileTerminologySource>();
        builder.Services.AddSingleton<StringTerminologySource>();

        // Terminology services
        builder.Services.AddSingleton<TerminologyService>();
        builder.Services.AddSingleton<TerminologyUpdaterService>();
        builder.Services.AddSingleton<TerminologySourcesRegistryService>();
    }

    public static void RegisterTransformerServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<RegistryMetadataTransformerService>();
        builder.Services.AddSingleton<JwtToSamlTransformerService>();
    }

    public static void RegisterXdsRegistryRepositoryServices(this WebApplicationBuilder builder)
    {
        var postgreSqlConnectionString = GetPostgreSqlConnectionString(builder.Configuration);
        var usePostgreSql = string.IsNullOrWhiteSpace(postgreSqlConnectionString) == false;

        // Registry
        builder.Services.AddScoped<XdsRegistryService>();
        builder.Services.AddSingleton<RegistryWrapper>();
        if (usePostgreSql)
        {
            builder.Services.AddSingleton<IRegistry, PostGreSqlBasedRegistry>();
        }
        else
        {
            builder.Services.AddSingleton<IRegistry, SqliteBasedRegistry>();
        }

        // Repository
        builder.Services.AddScoped<XdsRepositoryService>();
        builder.Services.AddSingleton<RepositoryWrapper>();
        if (usePostgreSql)
        {
            builder.Services.AddSingleton<IRepository, PostGreSqlBasedRepository>();
        }
        else
        {
            builder.Services.AddSingleton<IRepository, FileBasedRepository>();
        }

        // Miscellaneous services
        builder.Services.AddSingleton<IVirusScanner, ClamAvFileScanner>();
        builder.Services.AddSingleton<XdsSubmitObjectsValidator>();

        // Obfuscation of document lists
        builder.Services.AddSingleton<DocumentObfuscationService>();
        builder.Services.AddSingleton<DocumentListFiltererService>();
    }

    private static string? GetPostgreSqlConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("PostgreSql")
               ?? configuration["PostgreSql:ConnectionString"]
               ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
    }
}