using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using NHN.OpenTelemetryExtensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Collections;
using System.Text.Json.Serialization;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using XcaXds.WebService.InputFormatters;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;

namespace XcaXds.WebService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Begin builder

        builder.Logging.ClearProviders(); // Clear default logging providers
        builder.Services.AddLogging(logging =>
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            })
        );

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
        if (bool.Parse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false"))
        {
            var envVars = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value);
            var xdsConfigEnvVars = envVars.Where(n => n.Key.StartsWith("XdsConfiguration")).ToList();
            xdsConfig = ConfigBinder.BindKeyValueEnvironmentVariablesToXdsConfiguration(xdsConfigEnvVars);

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
        builder.Services.AddSingleton<PolicyRepositoryService>();
        builder.Services.AddSingleton<PolicyDecisionPointService>();
        builder.Services.AddScoped<Hl7RegistryService>();
        builder.Services.AddSingleton<RegistryWrapper>();
        builder.Services.AddSingleton<RepositoryWrapper>();
        builder.Services.AddSingleton<PolicyRepositoryWrapper>();
        builder.Services.AddSingleton<MonitoringStatusService>();
        builder.Services.AddSingleton<IRegistry, SqliteBasedRegistry>();
        builder.Services.AddSingleton<IRepository, FileBasedRepository>();
        builder.Services.AddSingleton<IPolicyRepository, FileBasedPolicyRepository>();
        builder.Services.AddSingleton<IAuditLogQueue, AuditLogQueue>();
        builder.Services.AddScoped<AuditLogGeneratorService>();
        builder.Services.AddHostedService<AuditLogExporterService>();


        // REST services
        builder.Services.AddScoped<RestfulRegistryRepositoryService>();

        // XDS On FHIR
        builder.Services.AddScoped<XdsOnFhirService>();

        builder.Services.AddHostedService<AppStartupService>();

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

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("nhn.xcads")
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri("collector.opentelemetry:4317");
                    })
                    .AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("System.Net.NameResolution");
            });


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

        var runningInContainer = bool.Parse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false");
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
