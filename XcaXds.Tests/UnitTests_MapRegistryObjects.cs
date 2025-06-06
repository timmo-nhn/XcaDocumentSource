using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;
using XcaXds.Source;

namespace XcaXds.Tests;

public class UnitTests_MapRegistryObjects
{
    [Fact]
    public async Task MapRegistryObjects()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("PnR")));
        var fileContent = await reader.ReadToEndAsync();

        var docc = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(fileContent);

        try
        {
            var documentEntryDto = new DocumentReferenceDto();
            var regmetaserv = new RegistryMetadataTransformerService();

            var registryObjectList = docc.Body.ProvideAndRegisterDocumentSetRequest.SubmitObjectsRequest.RegistryObjectList;


            var documentReference = regmetaserv.TransformRegistryObjectsToRegistryObjectDtos(registryObjectList.ToList());

            var registryObjects = regmetaserv.TransformRegistryObjectDtosToRegistryObjects(documentReference);

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
    public async Task MapEbRimRegistryObjectsRegistryToJsonDtoRegistry()
    {
        var rmts = new RegistryMetadataTransformerService();
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var registryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        var testDataFiles = Directory.GetFiles(registryPath);
        using var reader = File.OpenText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.xml")));

        var content = await reader.ReadToEndAsync();
        var registryContent = await sxmls.DeserializeSoapMessageAsync<XmlDocumentRegistry>(content);


        var documentDtoEntries = rmts.TransformRegistryObjectsToRegistryObjectDtos(registryContent.RegistryObjectList);

        var jsonRegistry = RegistryJsonSerializer.Serialize(documentDtoEntries);

        File.WriteAllText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json")), jsonRegistry);

    }
}
