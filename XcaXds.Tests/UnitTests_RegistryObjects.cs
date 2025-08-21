using System.Diagnostics;
using System.Security.Cryptography;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Serializers;
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

        var docc = sxmls.DeserializeSoapMessage<SoapEnvelope>(fileContent);

        try
        {
            var documentEntryDto = new DocumentReferenceDto();

            var registryObjectList = docc.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;


            var documentReference = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(registryObjectList?.ToList());

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
        var registryContent = sxmls.DeserializeSoapMessage<XmlDocumentRegistry>(content);


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


        var repoService = new FileBasedRepository(new ApplicationConfig() { RepositoryUniqueId = generatedTestRegistryObjects.OfType<DocumentEntryDto>().FirstOrDefault().RepositoryUniqueId });

        var registryService = new FileBasedRegistry();

        var documentRegistryObjects = registryService.ReadRegistry();
        foreach (var docentry in documentRegistryObjects.OfType<DocumentEntryDto>())
        {
            if (docentry.LegalAuthenticator == null) continue;

            docentry.LegalAuthenticator.IdSystem = Constants.Oid.Fnr;
        }

        registryService.WriteRegistry(documentRegistryObjects);


        foreach (var generatedTestObject in generatedTestRegistryObjects.OfType<DocumentEntryDto>())
        {
            var randomFileAsByteArray = files.ElementAt(new Random().Next(files.Count()));

            if (generatedTestObject?.PatientId?.Code != null && generatedTestObject.Id != null && randomFileAsByteArray != null)
            {
                generatedTestObject.Size = randomFileAsByteArray.Length.ToString();
                using (var md5 = MD5.Create())
                {
                    generatedTestObject.Hash = BitConverter.ToString(md5.ComputeHash(randomFileAsByteArray)).Replace("-", "");
                }
                repoService.Write(generatedTestObject.Id, randomFileAsByteArray, generatedTestObject.PatientId.Code);
            }
        }

    }

    [Fact]
    public async Task Registry_FindDudsInRegistryRepository()
    {
        var repoService = new FileBasedRepository(new ApplicationConfig() { RepositoryUniqueId = "2.16.578.1.12.4.5.100.1.2", HomeCommunityId = "2.16.578.1.12.4.5.100.1" });

        var registryService = new FileBasedRegistry();

        var registryContent = registryService.ReadRegistry();

        var documentEntries = registryContent.OfType<DocumentEntryDto>().ToList();

        List<string> duds = new();

        foreach (var documentEntry in documentEntries)
        {
            if (repoService.Read(documentEntry.Id) == null)
            {
                duds.Add($"{documentEntry.Id} for patient {documentEntry.SourcePatientInfo?.FirstName} {documentEntry.SourcePatientInfo?.LastName} (id: {documentEntry.PatientId.Code}) is a dud!!");
            }
        }

        Console.WriteLine(duds.Count + " duds!");
        if (duds.Any())
        {
            Console.WriteLine(duds.ToString());
        }
    }
}