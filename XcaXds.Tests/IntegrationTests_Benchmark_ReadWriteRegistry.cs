using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Testing;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Source;
using XcaXds.Tests.Helpers;
using Xunit.Abstractions;

namespace XcaXds.Tests;


public class IntegrationTests_Benchmark_ReadWriteRegistry : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_Benchmark_ReadWriteRegistry(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task RegistryBenchmark()
    {
        var statistics = "registryObjects;documentList;read;write\n";

        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "SoapRequests"));

        var iti38Request = File.ReadAllText(testDataFiles.First(f => f.Contains("iti38-iti40-request-kj.xml")));

        var sxmls = new SoapXmlSerializer();

        var registryObjectList = new List<RegistryObjectDto>();

        int registryCount = 0;

        while (registryCount < 1_000_00)
        {
            var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(10_000, patientId: null, true).AsRegistryObjectDtos().ToList();

            var swWrite = Stopwatch.StartNew();
            _registryWrapper.UpdateDocumentRegistryContentWithDtos(metadata);
            swWrite.Stop();

            var swRead = Stopwatch.StartNew();
            var fetchResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(iti38Request, Encoding.UTF8, Constants.MimeTypes.SoapXml));
            swRead.Stop();

            var response = await fetchResponse.Content.ReadAsStringAsync();
            var registryObjects = sxmls.DeserializeXmlString<SoapEnvelope>(response);

            var regObjects = registryObjects.Body.AdhocQueryResponse?.RegistryObjectList?.Length ?? 0;

            var count = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Count();

            statistics += ($"{count}; {regObjects}; {swRead.ElapsedMilliseconds}; {swWrite.ElapsedMilliseconds}\n");

            registryCount += 10_000;
        }

        _output.WriteLine(JsonSerializer.Serialize(statistics));
    }
}