
using XcaDocumentSource.Controllers;
using XcaDocumentSource.Services;
using XcaDocumentSource.Xca;

namespace XcaDocumentSource
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient();

            builder.Services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new SoapEnvelopeModelBinderProvider());
            });

            builder.Services.AddSingleton<RespondingGatewayController>();
            builder.Services.AddSingleton<RegistryController>();
            builder.Services.AddSingleton<RepositoryController>();
            builder.Services.AddSingleton<XcaGateway>();
            builder.Services.AddSingleton<SoapService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
