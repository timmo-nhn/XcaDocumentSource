using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
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
            _output.WriteLine("Must be run manually (will modify registry)");
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
            _output.WriteLine("Must be run manually (will modify registry)");
            return;
        }

        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));
        var testDataDocuments = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Documents"));

        using var testDataFile = File.OpenText(testDataFiles.First(f => f.Contains("TestDataRegistryObjects.json")));

        var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(await testDataFile.ReadToEndAsync());
        if (jsonTestData == null) return;

        jsonTestData.PossibleSubmissionSetValues.Authors = jsonTestData.PossibleDocumentEntryValues.Authors;
        
        var files = testDataDocuments.Select(file => File.ReadAllBytes(file)).ToList();

        var generatedTestRegistryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, 250);


        var repoService = new FileBasedRepository(new ApplicationConfig() { RepositoryUniqueId = generatedTestRegistryObjects.OfType<DocumentEntryDto>().FirstOrDefault().RepositoryUniqueId});

        var registryService = new FileBasedRegistry();

        foreach (var generatedTestObject in generatedTestRegistryObjects.OfType<DocumentEntryDto>())
        {
            var randomFileAsByteArray = files.ElementAt(new Random().Next(files.Count()));
            
            if (generatedTestObject?.PatientId?.Code != null && generatedTestObject.Id != null && randomFileAsByteArray != null)
            {
                generatedTestObject.Size = randomFileAsByteArray.Length.ToString();
                using (var md5 = MD5.Create())
                {
                    generatedTestObject.Hash  = BitConverter.ToString(md5.ComputeHash(randomFileAsByteArray)).Replace("-", "");
                }
                repoService.Write(generatedTestObject.Id, randomFileAsByteArray, generatedTestObject.PatientId.Code);
            }
        }

        var documentRegistryObjects = registryService.ReadRegistry();
        documentRegistryObjects.AddRange(generatedTestRegistryObjects);
        registryService.WriteRegistry(documentRegistryObjects);
    }
}