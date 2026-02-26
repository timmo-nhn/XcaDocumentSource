using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using NHN.OpenTelemetryExtensions;
using System.Collections;
using System.Text.Json.Serialization;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.DataManipulators;
using XcaXds.Source.Source;
using XcaXds.WebService.InputFormatters;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyBuilder;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyWriter;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;

namespace XcaXds.WebService;

public class Program
{
	public const long OneMb = 1L * 1024 * 1024;
	public const long FiftyMb = 50L * 1024 * 1024;
	public const long OneHundredMb = 100L * 1024 * 1024;
	public const long OneGb = 1L * 1024 * 1024 * 1024;

	public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var runningInContainer = bool.Parse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? bool.FalseString);

		// Begin builder

		builder.WebHost.ConfigureKestrel(options =>
		{
			// Upload of multiple huge documents should be done if separate requests and not in the same bundle
			// In addition to Kestrel limits, we also set limit per document in appsettings.XdsConfiguration.DocumentUploadSizeLimitKb

			options.Limits.MaxRequestBodySize = OneHundredMb; 			
		});
		
		builder.Logging.ClearProviders(); // Clear default logging providers
        builder.Services.AddLogging(logging =>
        {
            if (runningInContainer)
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

        builder.Services.AddHttpClient();

        builder.Services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new DocumentEntryDtoModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());
            options.InputFormatters.Insert(0, new Hl7InputFormatter());

        })
        .AddXmlSerializerFormatters()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                return ErrorResponseFactory.CreateErrorResponse(actionContext);
            };
        });

        builder.Configuration.AddEnvironmentVariables();

        var xdsConfig = new ApplicationConfig();

        // If we are running in a container, override appsettings.json and environment variables for configuration
        if (runningInContainer)
        {
            var envVars = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => (string?)entry.Value).ToList();
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

        // Register services
        builder.Services.AddScoped<XdsRegistryService>();
        builder.Services.AddScoped<XdsRepositoryService>();
        builder.Services.AddScoped<Hl7RegistryService>();
        builder.Services.AddScoped<AtnaLogGeneratorService>();

        builder.Services.AddScoped<PolicyEvaluator>();
        builder.Services.AddScoped<PolicyInputBuilder>();
        builder.Services.AddScoped<IPolicyInputStrategy,FhirJsonPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy,SoapSamlXmlPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy,JsonPolicyInputStrategy>();
        builder.Services.AddScoped<IPolicyInputStrategy, GenericPolicyInputStrategy>();

        builder.Services.AddScoped<PolicyDenyResponseBuilder>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, SoapDenyResponseStrategy>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, FhirDenyResponseStrategy>();
        builder.Services.AddScoped<IPepDenyResponseStrategy, JsonDenyResponseStrategy>();

        builder.Services.AddSingleton<PolicyRepositoryService>();
        builder.Services.AddSingleton<PolicyDecisionPointService>();
        builder.Services.AddSingleton<RegistryWrapper>();
        builder.Services.AddSingleton<RepositoryWrapper>();
        builder.Services.AddSingleton<PolicyRepositoryWrapper>();
        builder.Services.AddSingleton<MonitoringStatusService>();
        builder.Services.AddSingleton<RequestThrottlingService>();
        builder.Services.AddSingleton<IRegistry, SqliteBasedRegistry>();
        builder.Services.AddSingleton<IRepository, FileBasedRepository>();
        builder.Services.AddSingleton<IPolicyRepository, FileBasedPolicyRepository>();
        builder.Services.AddSingleton<IAtnaLogQueue, AtnaLogQueue>();
        
        builder.Services.AddHostedService<AtnaLogExporterService>();
        builder.Services.AddHostedService<AppStartupService>();

        // REST services
        builder.Services.AddScoped<RestfulRegistryRepositoryService>();

        // FHIR
        builder.Services.AddScoped<XdsOnFhirTransformer>();
        builder.Services.AddScoped<FhirService>();

        // Health check
        builder.Services.AddHealthChecks();

        // Database context
        builder.Services.AddDbContextFactory<SqliteRegistryDbContext>(options =>
            options.UseSqlite($"Data Source=\"{DatabasePathFinder.FindDatabasePath()}\""));

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

        // Begin app
        var app = builder.Build();
        app.UseExceptionHandler("/error");
        app.MapHealthChecks("/healthz");

        app.UseRouting();

        // Middleware, only enabled for endpoints with attributes
        app.UseMiddleware<SessionIdTraceMiddleware>();
        app.UseMiddleware<PolicyEnforcementPointMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            Console.WriteLine($"{entry.Key}={entry.Value}");
        }

        Console.WriteLine($"Running in container: {runningInContainer}");
        if (!runningInContainer)
        {
            builder.WebHost.UseUrls(["https://localhost:7176"]);
            app.UseHttpsRedirection();
        }

        app.MapControllers();

        app.Run();
    }
}
