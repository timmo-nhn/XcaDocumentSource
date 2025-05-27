using System.Text.Json;
using System.Text.Json.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Source;
using static XcaXds.Commons.Constants.Xds.Uuids;

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

            var association = registryObjectList.OfType<AssociationType>().FirstOrDefault();
            var extrinsicObject = registryObjectList.OfType<ExtrinsicObjectType>().FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var registryPackage = registryObjectList.OfType<RegistryPackageType>().FirstOrDefault(eo => eo.Id.NoUrn() == association.SourceObject.NoUrn());
            var document = docc.Body.ProvideAndRegisterDocumentSetRequest.Document.First(doc => doc.Id == extrinsicObject.Id);

            var documentReference = regmetaserv.TransformRegistryObjectsToDocumentEntryDto(extrinsicObject, registryPackage, association);

            var registryObjects = regmetaserv.TransformDocumentEntryDtoToRegistryObjects(documentReference);

            var outputSoap = new SoapEnvelope()
            {
                Header = docc.Header,
                Body = new()
                {
                    ProvideAndRegisterDocumentSetRequest = new()
                    {
                        SubmitObjectsRequest = new() { RegistryObjectList = registryObjects }
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
        var registryContent = await sxmls.DeserializeSoapMessageAsync<DocumentRegistry>(content);

        var associations = registryContent.RegistryObjectList.OfType<AssociationType>()
            .Where(assoc => assoc.AssociationTypeData == Constants.Xds.AssociationType.HasMember).ToArray();
        var extrinsicObjects = registryContent.RegistryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = registryContent.RegistryObjectList.OfType<RegistryPackageType>().ToArray();

        var documentDtoEntries = new List<DocumentReferenceDto>();

        foreach (var association in associations)
        {
            var extrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var registryPackage = registryPackages.FirstOrDefault(rp => rp.Id.NoUrn() == association.SourceObject.NoUrn());

            var documentEntryDto = rmts.TransformRegistryObjectsToDocumentEntryDto(extrinsicObject, registryPackage, association, null);
            documentDtoEntries.Add(documentEntryDto);
            
        }

        var jsonRegistry = JsonSerializer.Serialize(documentDtoEntries, new JsonSerializerOptions() { WriteIndented = true});
        File.WriteAllText(testDataFiles.FirstOrDefault(f => f.Contains("Registry.json")),jsonRegistry);

    }

}
