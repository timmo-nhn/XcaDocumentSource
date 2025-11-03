using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using Xunit.Abstractions;

namespace XcaXds.Tests;


public class IntegrationTests_Benchmark_ReadWriteRegistry : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;
    private readonly ApplicationConfig _applicationConfig;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly ITestOutputHelper _output;

    public IntegrationTests_Benchmark_ReadWriteRegistry(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        _applicationConfig = scope.ServiceProvider.GetRequiredService<ApplicationConfig>();
        _repositoryWrapper = scope.ServiceProvider.GetRequiredService<RepositoryWrapper>();
        _output = output;

    }

    [Fact]
    public async Task RegistryBenchmark()
    {
        var statistics = new List<object>();
        var registry = new FileBasedRegistry();
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var iti38Request = File.ReadAllText(testDataFiles.First(f => f.Contains("iti38-iti40-request-kj.xml")));

        var testDataFile = File.ReadAllText(testDataFiles.First(f => f.Contains("TestDataRegistryObjects.json")));

        var sxmls = new SoapXmlSerializer();

        var registryObjectList = new List<RegistryObjectDto>();

        for (int i = 0; i < 200; i++)
        {
            var swRead = Stopwatch.StartNew();
            var uploadResponse = await _client.PostAsync("/api/generate-test-data?patientIdentifier=13116900216&entriesToGenerate=50", new StringContent(testDataFile, Encoding.UTF8, Constants.MimeTypes.Json));
            swRead.Stop();
            
            var swWrite = Stopwatch.StartNew();
            var fetchResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(iti38Request, Encoding.UTF8, Constants.MimeTypes.SoapXml));
            swWrite.Stop();

            var response = await fetchResponse.Content.ReadAsStringAsync();
            var registryObjects = sxmls.DeserializeXmlString<SoapEnvelope>(response);

            var regObjects = registryObjects.Body?.AdhocQueryResponse?.RegistryObjectList?.Length ?? 0;
            Debug.WriteLine(i);
            Debug.WriteLine(regObjects);
            statistics.Add(new { RegistryObjects = regObjects, Read=swRead.ElapsedMilliseconds, Write= swWrite.ElapsedMilliseconds });
        }

        _output.WriteLine(JsonSerializer.Serialize(statistics));
    }
}