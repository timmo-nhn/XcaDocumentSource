using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Services;

namespace XcaXds.Tests;

public class UnitTests_RegistryObjects
{
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
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var registryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        var testDataFiles = Directory.GetFiles(registryPath);
        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.xml")));

        var content = await reader.ReadToEndAsync();
        var registryContent = await sxmls.DeserializeSoapMessageAsync<XmlDocumentRegistry>(content);


        var documentDtoEntries = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(registryContent.RegistryObjectList);

        var jsonRegistry = RegistryJsonSerializer.Serialize(documentDtoEntries);

        File.WriteAllText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json")), jsonRegistry);

    }

    [Fact]
    public async Task Registry_ReadWriteRegistry()
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var registryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        var testDataFiles = Directory.GetFiles(registryPath);
        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json.backup")));
        var jsonContent = await reader.ReadToEndAsync();
        var content = RegistryJsonSerializer.Deserialize<List<RegistryObjectDto>>(jsonContent);

        var jsonRegistry = RegistryJsonSerializer.Serialize(content);

        File.WriteAllText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json")), jsonRegistry);
    }
}
