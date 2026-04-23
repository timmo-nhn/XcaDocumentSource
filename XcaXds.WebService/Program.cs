using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using NHN.OpenTelemetryExtensions;
using System.Collections;
using System.Text.Json.Serialization;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Source.Source;
using XcaXds.WebService.InputFormatters;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;
using XcaXds.WebService.Services.PolicyEnforcementPoint;
using XcaXds.WebService.Services.PolicyEnforcementPoint.DenyBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint.DenyStrategies;
using XcaXds.WebService.Services.PolicyEnforcementPoint.InputBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;
using XcaXds.WebService.Startup;

namespace XcaXds.WebService;

public class Program
{
    public const long OneMb = 1L * 1024 * 1024;
    public const long FiftyMb = 50L * 1024 * 1024;
    public const long OneHundredMb = 100L * 1024 * 1024;
    public const long OneGb = 1L * 1024 * 1024 * 1024;

    private static readonly bool RunningInContainer = bool.Parse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? bool.FalseString);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            // Upload of multiple huge documents should be done if separate requests and not in the same bundle
            // In addition to Kestrel limits, we also set limit per document in appsettings.XdsConfiguration.DocumentUploadSizeLimitKb
            options.Limits.MaxRequestBodySize = OneHundredMb;
        });

        ConfigureLoggingOptions(builder);

        builder.Services.AddHttpClient();
        builder.Configuration.AddEnvironmentVariables();

        AddControllersAndModelBindings(builder);

        ConfigureKestrelCertificateAuthenticationAuthorization(builder);

        AddModelValidationHandling(builder);

        RegisterDependencyInjectionServices(builder);

        RegisterHostedServices(builder);

        // Feature Toggle (located in XcaXds.WebService/appsettings.json)
        builder.Services.AddFeatureManagement();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.SetupOpenTelemetryDHP();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("_allowSpecificOrigins",
                policy =>
                {
                    //policy.WithOrigins($"https://localhost:{ConfigurationValues.SampleApiPort}").AllowAnyHeader();
                    policy.WithOrigins($"*").AllowAnyHeader();
                });
        });

        var app = builder.Build();
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

        app.Run();
    }

    private static void RegisterHostedServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<AtnaLogExporterService>();
        builder.Services.AddHostedService<AppStartupService>();
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
            builder.WebHost.UseUrls(["https://localhost:7176"]);
            //app.UseHttpsRedirection();
        }
    }

    private static void AddModelValidationHandling(WebApplicationBuilder builder)
    {
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                return ErrorResponseFactory.CreateErrorResponse(actionContext);
            };
        });
    }

    private static void AddControllersAndModelBindings(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new DocumentEntryDtoModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());
            options.InputFormatters.Insert(0, new Hl7InputFormatter());
        })
        .AddXmlSerializerFormatters()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    private static void RegisterMiddlewareForApplication(WebApplication app)
    {
        app.UseMiddleware<SessionIdTraceMiddleware>();
        app.UseMiddleware<SoapServiceStatisticsMiddleware>();

        // Middleware below will only enabled for endpoints with attributes
        app.UseMiddleware<PolicyEnforcementPointMiddleware>();
        app.UseMiddleware<AtnaAuditLoggingMiddleware>();
    }

    private static void RegisterDependencyInjectionServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<XdsRegistryService>();
        builder.Services.AddScoped<XdsRepositoryService>();
        builder.Services.AddScoped<Hl7RegistryService>();
        builder.Services.AddScoped<AtnaLogGeneratorService>();

        builder.Services.AddScoped<PolicyEvaluator>();
        builder.Services.AddScoped<PolicyInputBuilder>();
        builder.Services.AddScoped<IPolicyInputStrategy, FhirJsonPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, SoapSamlXmlPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, JsonPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, GenericPolicyInputStrategy>();

        builder.Services.AddScoped<PolicyDenyResponseBuilder>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, SoapDenyResponseStrategy>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, FhirDenyResponseStrategy>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, JsonDenyResponseStrategy>();

        builder.Services.AddScoped<AtnaLogBuilder>();
        builder.Services.AddScoped<IAtnaLogStrategy, SoapEnvelopeStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirPatchDocumentStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirDeleteDocumentsStrategy>();
        builder.Services.AddScoped<IAtnaLogStrategy, FhirProvideBundleStrategy>();

        builder.Services.AddSingleton<AtnaLogEnricherService>();
        builder.Services.AddSingleton<PolicyRequestMapperSamlService>();
        builder.Services.AddSingleton<PolicyRequestMapperJsonWebTokenService>();

        builder.Services.AddSingleton<XdsSubmitObjectsValidator>();

        builder.Services.AddSingleton<IClamAvFileScanner, ClamAvFileScanner>();
        builder.Services.AddSingleton<ApplicationMetaService>();
        builder.Services.AddSingleton<PolicyRepositoryService>();
        builder.Services.AddSingleton<MonitoringStatusService>();
        builder.Services.AddSingleton<PolicyDecisionPointService>();
        builder.Services.AddSingleton<RegistryWrapper>();
        builder.Services.AddSingleton<RepositoryWrapper>();
        builder.Services.AddSingleton<PolicyRepositoryWrapper>();
        builder.Services.AddSingleton<RequestThrottlingService>();
        builder.Services.AddSingleton<IRegistry, SqliteBasedRegistry>();
        builder.Services.AddSingleton<IRepository, FileBasedRepository>();
        builder.Services.AddSingleton<IPolicyRepository, FileBasedPolicyRepository>();
        builder.Services.AddSingleton<IAtnaLogQueue, AtnaLogQueue>();

        // Custom REST services
        builder.Services.AddScoped<RestfulRegistryRepositoryService>();

        // FHIR
        builder.Services.AddScoped<FhirService>();
        builder.Services.AddSingleton<FhirResourceValidatorService>();

        // Health check
        builder.Services.AddHealthChecks();

        // Database context
        builder.Services.AddDbContextFactory<SqliteRegistryDbContext>(options =>
            options.UseSqlite($"Data Source=\"{DatabasePathFinder.FindDatabasePath()}\"",
            sqliteOptions => sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        var xdsConfig = new ApplicationConfig();

        // If we are running in a container, override appsettings.json and environment variables for configuration
        if (RunningInContainer)
        {
            var envVars = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Select(e => new KeyValuePair<string, string>((string)e.Key, (string)e.Value!))
                .ToList();

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
        }

        builder.Services.AddSingleton(xdsConfig);

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
                logging.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                });
            }
        });

        builder.Logging.SetMinimumLevel(LogLevel.Debug);
    }

    private static void ConfigureKestrelCertificateAuthenticationAuthorization(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
            });
        });

        builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CertificateAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CertificateAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCertificate(options =>
        {
            options.Events = new CertificateAuthenticationEvents
            {
                OnCertificateValidated = context =>
                {
                    var cert = context.ClientCertificate;

                    if (cert.Subject.Contains("CN=trusted-client"))
                    {
                        context.Success();
                    }
                    else
                    {
                        context.Fail("Invalid certificate");
                    }

                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ClientCertificatePolicy", policy =>
            {
                policy.AddAuthenticationSchemes(CertificateAuthenticationDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();
            });
        });
    }
}
