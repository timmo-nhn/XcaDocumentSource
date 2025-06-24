using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;
using Xunit.Abstractions;

namespace XcaXds.Tests;

public class UnitTests_RegistryObjects
{
    private readonly ITestOutputHelper _output;

    public UnitTests_RegistryObjects(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Registry_MapRegistryObjects()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("PnR")));
        var fileContent = await reader.ReadToEndAsync();

        var docc = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(fileContent);

        try
        {
            var documentEntryDto = new DocumentReferenceDto();

            var registryObjectList = docc.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;


            var documentReference = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(registryObjectList.ToList());

            var registryObjects = RegistryMetadataTransformerService.TransformRegistryObjectDtosToRegistryObjects(documentReference);

            var outputSoap = new SoapEnvelope()
            {
                Header = docc.Header,
                Body = new()
                {
                    ProvideAndRegisterDocumentSetRequest = new()
                    {
                        SubmitObjectsRequest = new() { RegistryObjectList = registryObjects.ToArray() }
                    }
                }
            };

            var xmlstring = sxmls.SerializeSoapMessageToXmlString(outputSoap).Content;

        }
        catch (Exception)
        {
            throw;
        }
    }

    [Fact]
    public async Task Registry_MapEbRimRegistryObjectsRegistryToJsonDtoRegistry()
    {
        if (!Debugger.IsAttached)
        {
            _output.WriteLine("Skipping manual test (not debugging).");
            return;
        }

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var registryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        var testDataFiles = Directory.GetFiles(registryPath);
        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.xml")));

        var content = await reader.ReadToEndAsync();
        var registryContent = await sxmls.DeserializeSoapMessageAsync<XmlDocumentRegistry>(content);


        var documentDtoEntries = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(registryContent.RegistryObjectList);

        var jsonRegistry = RegistryJsonSerializer.Serialize(documentDtoEntries);

        //File.WriteAllText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json")), jsonRegistry);
    }

    [Fact]
    public async Task Registry_ReadWriteRegistry()
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var registryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        var testDataFiles = Directory.GetFiles(registryPath);
        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json")));
        var jsonContent = await reader.ReadToEndAsync();
        var content = RegistryJsonSerializer.Deserialize<List<RegistryObjectDto>>(jsonContent);

        var jsonRegistry = RegistryJsonSerializer.Serialize(content);

        Assert.Equal(jsonRegistry, jsonContent);
    }

    [Fact]
    public async Task Registry_GenerateTestData()
    {
        if (!Debugger.IsAttached)
        {
            _output.WriteLine("Skipping manual test (not debugging).");
            return;
        }

        var registryFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry"));
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));
        var testDataDocuments = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Documents"));

        using var registryFile = File.OpenText(registryFiles.First(f => f.Contains("Registry.json")));
        using var testDataFile = File.OpenText(testDataFiles.First(f => f.Contains("TestDataRegistryObjects.json")));

        var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(await testDataFile.ReadToEndAsync());
        if (jsonTestData == null) return;

        jsonTestData.PossibleSubmissionSetValues.Authors = jsonTestData.PossibleDocumentEntryValues.Authors;
        
        var registryContent = RegistryJsonSerializer.Deserialize<List<RegistryObjectDto>>(await registryFile.ReadToEndAsync());

        var files = testDataDocuments.Select(file => File.ReadAllText(file)).ToList();

        var registryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, 100);

        var repoService = new FileBasedRepository(new ApplicationConfig() { RepositoryUniqueId = registryObjects.OfType<DocumentEntryDto>().FirstOrDefault().RepositoryUniqueId});

        foreach (var registryObject in registryObjects.OfType<DocumentEntryDto>())
        {
            var randomFileAsByteArray = Encoding.UTF8.GetBytes(files.ElementAt(new Random().Next(files.Count())));
            
            if (registryObject?.PatientId?.Code != null && registryObject.Id != null && randomFileAsByteArray != null)
            {
                repoService.Write(registryObject.Id, randomFileAsByteArray, registryObject.PatientId.Code);
            }
        }
                        
        var jsonRegistry = RegistryJsonSerializer.Serialize(registryContent);

        File.WriteAllText(registryFiles.First(f => f.Contains("Registry.json")), jsonRegistry);
    }
}