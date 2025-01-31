using Microsoft.AspNetCore.Mvc.Formatters;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Source;
using XcaXds.Source.Services;
using XcaXds.WebService.Controllers;

namespace XcaXds.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient();

            builder.Services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());

                options.InputFormatters.Insert(0, new XmlSerializerInputFormatter(options));
                options.OutputFormatters.Insert(0, new XmlSerializerOutputFormatter());
            })
            .AddXmlSerializerFormatters();


            var xdsConfig = new XdsConfig();

            builder.Configuration.GetSection("XdsConfiguration").Bind(xdsConfig);

            builder.Services.AddSingleton<RespondingGatewayController>();
            builder.Services.AddSingleton<RegistryController>();
            builder.Services.AddSingleton<RepositoryController>();
            builder.Services.AddSingleton<XcaGateway>();
            builder.Services.AddSingleton<SoapService>();
            builder.Services.AddSingleton<RepositoryService>();
            builder.Services.AddSingleton<RepositoryWrapper>();
            builder.Services.AddSingleton<RegistryService>();
            builder.Services.AddSingleton<RegistryWrapper>();
            builder.Services.AddSingleton(xdsConfig);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
