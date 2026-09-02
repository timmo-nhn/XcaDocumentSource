using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi;
using NHN.OpenTelemetryExtensions;
using System.Collections;
using System.Text.Json.Serialization;
using XcaXds.Commons.Models.Custom.ApiKey;
using XcaXds.Shared;
using XcaXds.Shared.ConfigBinder;
using XcaXds.Source.Implementations.RegistryRepository.PostGreSql;
using XcaXds.Source.Implementations.RegistryRepository.SqLite;
using XcaXds.Source.Implementations.Statistics.PostGreSql;
using XcaXds.Terminology.Services;
using XcaXds.WebService.AuthenticationHandler;
using XcaXds.WebService.Extensions;
using XcaXds.WebService.HealthChecks;
using XcaXds.WebService.InputFormatters;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.AtnaAuditLogging;
using XcaXds.WebService.Services.Statistics;
using XcaXds.WebService.Startup;

namespace XcaXds.WebService;

public class Program
{
    private static readonly bool RunningInContainer =
        bool.Parse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? bool.FalseString);

    public static async Task Main(string[] args)
    {
        var runMigrationsOnly = args.Any(arg => string.Equals(arg, "--migrate-only", StringComparison.OrdinalIgnoreCase));
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            // Upload of multiple huge documents should be done if separate requests and not in the same bundle
            // In addition to Kestrel limits, we also set limit per document in appsettings.XdsConfiguration.DocumentUploadSizeLimitKb
            options.Limits.MaxRequestBodySize = Constants.FileSizes.OneHundredMb;
        });

        ConfigureLoggingOptions(builder);

        builder.Services.AddHttpClient();
        builder.Configuration.AddEnvironmentVariables();

        AddControllersAndModelBindings(builder);

        ConfigureKestrelAuthenticationAuthorization(builder);

        AddModelValidationHandling(builder);

        RegisterDependencyInjectionServices(builder);

        AddDatabaseConfiguration(builder);

        RegisterHostedServices(builder);

        // Feature Toggle (located in XcaXds.WebService/appsettings.json)
        builder.Services.AddFeatureManagement();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "X-API-KEY", // Replace with your actual header name (e.g., "Authorization", "api_key")
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme",
                In = ParameterLocation.Header,
                Description = "Enter your API key into the field below. Example: Bearer {token} or simply your key."
            };

            options.AddSecurityDefinition("ApiKeyScheme", securityScheme);
            options.OperationFilter<RequiresApiKeyOperationFilter>();
        });

        builder.SetupOpenTelemetryDHP();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("_allowSpecificOrigins",
                policy =>
                {
                    //policy.WithOrigins($"https://localhost:{ConfigurationValues.SampleApiPort}").AllowAnyHeader();
                    policy.AllowAnyOrigin().AllowAnyHeader();
                });
        });

        var app = builder.Build();

        if (runMigrationsOnly)
        {
            await RunMigrationsOnlyAsync(app);
            return;
        }

        app.UseExceptionHandler("/error");
        app.MapHealthChecks("/healthz");

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        RegisterMiddlewareForApplication(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            DebuggingBeforeAppLaunch(builder);
        }

        app.MapControllers();

        await InitializeServicesBeforeAppRunAsync(builder, app);

        app.Run();
    }

    private static async Task InitializeServicesBeforeAppRunAsync(WebApplicationBuilder builder, WebApplication app)
    {
        var implementationInformer = app.Services.GetRequiredService<ImplementationInformerService>();
        implementationInformer.Initialize(builder);

        var terminologyUpdater = app.Services.GetRequiredService<TerminologyUpdaterService>();
        await terminologyUpdater.InitializeAsync(CancellationToken.None);
    }

    private static async Task RunMigrationsOnlyAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MigrationRunner");
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var configuredRegistryBackend = configuration["XdsConfiguration:RegistryBackend"];
        var postgreSqlConnectionString = configuration.GetPostgreSqlConnectionString();
        var usePostgreSqlRegistry = ShouldUsePostgreSqlRegistryForMigration(configuredRegistryBackend, postgreSqlConnectionString);

        if (usePostgreSqlRegistry)
        {
            var postgreSqlFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PostGreSqlRegistryDbContext>>();
            await using var context = await postgreSqlFactory.CreateDbContextAsync();
            logger.LogInformation("Applying PostgreSQL registry migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("PostgreSQL registry migrations applied successfully.");

            var statisticsFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StatisticsDbContext>>();
            await using var statisticsContext = await statisticsFactory.CreateDbContextAsync();
            logger.LogInformation("Applying PostgreSQL statistics migrations...");
            await statisticsContext.Database.MigrateAsync();
            logger.LogInformation("PostgreSQL statistics migrations applied successfully.");

        }
        else
        {
            var sqliteFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SqliteRegistryDbContext>>();
            await using var sqliteContext = await sqliteFactory.CreateDbContextAsync();
            logger.LogInformation("Applying SQLite registry migrations...");
            await sqliteContext.Database.MigrateAsync();
            logger.LogInformation("SQLite registry migrations applied successfully.");
        }
    }

    private static bool ShouldUsePostgreSqlRegistryForMigration(string? configuredRegistryBackend, string? postgreSqlConnectionString)
    {
        if (string.Equals(configuredRegistryBackend, "postgresql", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(configuredRegistryBackend, "sqlite", StringComparison.OrdinalIgnoreCase))
            return false;

        return RunningInContainer || string.IsNullOrWhiteSpace(postgreSqlConnectionString) == false;
    }

    private static void AddDatabaseConfiguration(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<SqliteRegistryDbContext>(options =>
            options.UseSqlite($"Data Source=\"{DatabasePathFinder.FindDatabasePath()}\"",
                sqliteOptions => sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        var postgreSqlConnectionString = builder.Configuration.GetPostgreSqlConnectionString();
        if (string.IsNullOrWhiteSpace(postgreSqlConnectionString))
        {
            return;
        }

        builder.Services.AddDbContextFactory<PostGreSqlRegistryDbContext>(options =>
            options.UseNpgsql(postgreSqlConnectionString,
                npgsqlOptions => npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        builder.Services.AddDbContextFactory<StatisticsDbContext>(options =>
            options.UseNpgsql(postgreSqlConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Statistics")));
    }

    private static void RegisterHostedServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<AppStartupService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<AtnaLogExporterService>());
        builder.Services.AddHostedService<StatisticsProcessorService>();
    }

    private static void DebuggingBeforeAppLaunch(WebApplicationBuilder builder)
    {
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            Console.WriteLine($"{entry.Key}={entry.Value}");
        }

        Console.WriteLine($"Running in container: {RunningInContainer}");
        if (!RunningInContainer)
        {
            builder.WebHost.UseUrls("https://localhost:7176");
            //app.UseHttpsRedirection();
        }
    }

    private static void AddModelValidationHandling(WebApplicationBuilder builder)
    {
        builder.Services.Configure<ApiBehaviorOptions>(options => { options.InvalidModelStateResponseFactory = ErrorResponseFactory.CreateErrorResponse; });
    }

    private static void AddControllersAndModelBindings(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new DocumentEntryDtoModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());
            })
            .AddXmlSerializerFormatters()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    private static void RegisterMiddlewareForApplication(WebApplication app)
    {
        app.UseMiddleware<SessionIdTraceMiddleware>();
        app.UseMiddleware<RequestStatisticsMiddleware>();

        // Middleware below will only be enabled for endpoints with attributes
        app.UseMiddleware<AtnaAuditLoggingMiddleware>();
        app.UseMiddleware<PolicyEnforcementPointMiddleware>();
    }

    private static void RegisterDependencyInjectionServices(WebApplicationBuilder builder)
    {
        builder.RegisterAuditLoggingServices();
        builder.RegisterBusinessLogicServices();
        builder.RegisterMetaAndStatusServices();
        builder.RegisterFhirServices();
        builder.RegisterNinServices();
        builder.RegisterPolicyEnforcementPointServices();
        builder.RegisterStatisticsServices();
        builder.RegisterTerminologyServices();
        builder.RegisterTransformerServices();
        builder.RegisterXdsRegistryRepositoryServices();


        // Custom REST services
        builder.Services.AddScoped<RestfulRegistryRepositoryService>();


        // Health check
        builder.Services.AddHealthChecks()
            .AddCheck<RegistryHealthCheck>("registry", tags: ["registry", "ready"])
            .AddCheck<RepositoryHealthCheck>("repository", tags: ["repository", "ready"])
            .AddCheck<AtnaLogExportHealthCheck>("atnalogexport", tags: ["atna", "ready"]);

        var xdsConfig = new ApplicationConfig();
        var apiKey = new ApiKeyHolder();

        // If we are running in a container, override appsettings.json and environment variables for configuration
        if (RunningInContainer)
        {
            var envVars = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Select(e => new KeyValuePair<string, string>((string)e.Key, (string)e.Value!))
                .ToDictionary();

            foreach (var var in envVars)
            {
                Console.WriteLine($"{var.Key}: {var.Value}");
            }

            apiKey = ApiKeyBinder.BindApiKeyEnvironmentVariablesToApiKey(envVars);
            xdsConfig = ConfigBinder.BindKeyValueEnvironmentVariablesToXdsConfiguration(envVars);

            builder.Configuration.Bind(xdsConfig);
            Environment.SetEnvironmentVariable("TMP", @"/mnt/data/tmp", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("TEMP", @"/mnt/data/tmp", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("TMPDIR", @"/mnt/data/tmp", EnvironmentVariableTarget.Process);

            Console.WriteLine(Path.GetTempPath()); // now returns /mnt/data/tmp/
        }
        else
        {
            builder.Configuration.GetSection("XdsConfiguration").Bind(xdsConfig);
            builder.Configuration.GetSection("XdsConfiguration").Bind(apiKey);
        }
        NormalizeDelimitedArrayConfig(builder, xdsConfig);
        builder.Services.AddSingleton(xdsConfig);
        builder.Services.AddSingleton(apiKey);
    }

    private static void NormalizeDelimitedArrayConfig(WebApplicationBuilder builder, ApplicationConfig xdsConfig)
    {
        var xdsConfigurationSection = builder.Configuration.GetSection("XdsConfiguration");

        xdsConfig.SamlValidationCertificatesRaw = GetDelimitedArrayOrFallback(xdsConfigurationSection["CertificatesRaw"], xdsConfig.SamlValidationCertificatesRaw);
        xdsConfig.SamlValidationSigningCertificateUrls = GetDelimitedArrayOrFallback(xdsConfigurationSection["SigningCertificateUrls"], xdsConfig.SamlValidationSigningCertificateUrls);
        xdsConfig.SamlValidationValidAudiences = GetDelimitedArrayOrFallback(xdsConfigurationSection["ValidAudiences"], xdsConfig.SamlValidationValidAudiences);
        xdsConfig.SamlValidationValidIssuers = GetDelimitedArrayOrFallback(xdsConfigurationSection["ValidIssuers"], xdsConfig.SamlValidationValidIssuers);
    }

    private static string[] GetDelimitedArrayOrFallback(string? rawValue, string[]? fallback)
    {
        if (!string.IsNullOrWhiteSpace(rawValue))
        {
            return rawValue
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        return fallback ?? [];
    }

    private static void ConfigureLoggingOptions(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders(); // Clear default logging providers
        builder.Services.AddLogging(logging =>
        {
            if (RunningInContainer)
            {
                logging.AddJsonConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                });
            }
            else
            {
                logging.AddSimpleConsole(options => { options.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; });
            }
        });
    }

    public static void ConfigureKestrelAuthenticationAuthorization(WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel();

        builder.Services
            .AddAuthentication("ApiKey")
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

        builder.Services.AddAuthorization();
    }
}