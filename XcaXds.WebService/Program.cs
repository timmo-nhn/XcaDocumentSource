using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.OpenDipsRegistryRepository.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using XcaXds.WebService.InputFormatters;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Startup;

namespace XcaXds.WebService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // Begin builder
        builder.WebHost.UseUrls(["https://localhost:7176", "http://localhost:5009"]);

        builder.Logging.ClearProviders(); // Clear default logging providers
        builder.Services.AddLogging(logging =>
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            })
        );


        builder.Services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new DocumentEntryDtoModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());
            options.InputFormatters.Insert(0, new Hl7InputFormatter());
            //options.InputFormatters.Insert(0, new XmlSerializerInputFormatter(options));
            //options.OutputFormatters.Insert(0, new XmlSerializerOutputFormatter());
        })
        .AddXmlSerializerFormatters();


        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                return ErrorResponseFactory.CreateErrorResponse(actionContext);
            };
        });


        var xdsConfig = new ApplicationConfig();

        builder.Configuration.GetSection("XdsConfiguration").Bind(xdsConfig);

        builder.Services.AddHttpClient<SoapService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(xdsConfig.TimeoutInSeconds);
        });


        // Register services
        builder.Services.AddScoped<XcaGateway>();
        //builder.Services.AddSingleton<SoapService>();
        builder.Services.AddScoped<XdsRepositoryService>();
        builder.Services.AddScoped<XdsRegistryService>();
        builder.Services.AddScoped<Hl7RegistryService>();
        builder.Services.AddSingleton<RepositoryWrapper>();
        builder.Services.AddSingleton<RegistryWrapper>();
        builder.Services.AddSingleton<IRegistry, FileBasedRegistry>();
        builder.Services.AddSingleton<IRepository, FileBasedRepository>();
        builder.Services.AddSingleton(xdsConfig);

        // OpenDips service
        builder.Services.AddSingleton<OpenDipsClient>();
        builder.Services.AddSingleton<OpenDipsTokenService>();

        // Fhir server interfacing service
        builder.Services.AddSingleton<IFhirEndpointsService, OpenDipsClient>();

        builder.Services.AddSingleton(provider =>
        new FhirEndpointsDtoTransformerService(
            "http://hapi.fhir.org/baseR4",
            provider.GetService<ILogger<FhirEndpointsDtoTransformerService>>(),
            provider.GetService<IFhirEndpointsService>()));


        // REST services
        builder.Services.AddScoped<RestfulRegistryRepositoryService>();

        // XDS On FHIR service
        builder.Services.AddScoped<XdsOnFhirService>();

        builder.Services.AddHostedService<AppStartupService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Feature Toggle (located in XcaXds.WebService/Appsettings.json)
        builder.Services.AddFeatureManagement();

        // Health check
        builder.Services.AddHealthChecks();

        // Begin app
        var app = builder.Build();
        app.UseExceptionHandler("/error");
        app.MapHealthChecks("/healthz");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Middleware, only enabled for endpoints with attributes
        app.UseMiddleware<PolicyEnforcementPointMiddlware>();
        app.UseMiddleware<AuditLoggingMiddleware>();

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}
