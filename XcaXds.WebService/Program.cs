using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Source;
using XcaXds.Source.Services;
using XcaXds.WebService.InputFormatters;

namespace XcaXds.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Begin builder
            builder.Services.AddHttpClient();

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
                options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());
                options.InputFormatters.Insert(0, new XmlSerializerInputFormatter(options));
                options.OutputFormatters.Insert(0, new XmlSerializerOutputFormatter());
            })
            .AddXmlSerializerFormatters();


            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                { 
                    return SoapFaultErrorResponseFactory.CreateErrorResponse(actionContext); 
                };
            });


            var xdsConfig = new XdsConfig();

            builder.Configuration.GetSection("XdsConfiguration").Bind(xdsConfig);


            // Register services
            builder.Services.AddSingleton<XcaGateway>();
            builder.Services.AddSingleton<SoapService>();
            builder.Services.AddSingleton<RepositoryService>();
            builder.Services.AddSingleton<RepositoryWrapper>();
            builder.Services.AddSingleton<RegistryService>();
            builder.Services.AddSingleton<RegistryWrapper>();
            builder.Services.AddSingleton(xdsConfig);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Begin app
            var app = builder.Build();
            app.UseExceptionHandler("/error");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
