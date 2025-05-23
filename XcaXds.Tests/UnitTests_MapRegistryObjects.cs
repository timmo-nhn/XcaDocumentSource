using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

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

            var documentReference = regmetaserv.TransformRegistryObjectsToDocumentEntryDto(extrinsicObject, registryPackage, document, association);

            var registryObjects = regmetaserv.TransformDocumentEntryDtoToRegistryObjects(documentReference);

        }
        catch (Exception)
        {
            throw;
        }
    }
}
