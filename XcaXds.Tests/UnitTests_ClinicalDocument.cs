using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.DataManipulators;
using XcaXds.Source.Source;
using XcaXds.Tests.Helpers;

namespace XcaXds.Tests;

public class UnitTests_ClinicalDocument
{
    [Fact]
    public async Task CDA_SerializeDeserialize()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        foreach (var file in testDataFiles)
        {
            if (!file.Contains("CDA")) continue;

            var fileContent = File.ReadAllText(file);

            var docc = sxmls.DeserializeXmlString<ClinicalDocument>(fileContent);

            var doccCDA = sxmls.SerializeSoapMessageToXmlString(docc);

            int file1 = fileContent.Split("\n").Length;
            int file2 = doccCDA.Content.Split("\n").Length;
            int diff = file1 - file2;
            Assert.InRange(diff, 0, 3);
        }
    }

    //[Fact]
    //public async Task TransformCdaToIti41()
    //{
    //    var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

    //    var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

    //    foreach (var file in testDataFiles)
    //    {
    //        var fileContent = string.Empty;

    //        if (file.Contains("CDA_Level1"))
    //        {
    //            fileContent = File.ReadAllText(file);
    //        }
    //        else continue;

    //        var docc = await sxmls.DeserializeSoapMessageAsync<ClinicalDocument>(fileContent);

    //        var registryobjects = _transformerService.TransformCdaDocumentToRegistryObjectList(docc);

    //    }
    //}


    [Fact]
    public async Task TransformRegistryObjectDtosToCda()
    {
        var registryObjects = TestHelpers.GenerateComprehensiveRegistryMetadata("13116900216");

        var registryMetadata = registryObjects.AsRegistryObjectList();
        var documents = registryObjects.Select(ro => ro.Document);

        var randomIndex = new Random().Next(registryObjects.OfType<DocumentEntryDto>().Count());

        var documentEntry = registryMetadata.OfType<DocumentEntryDto>().ElementAt(randomIndex);
        var association = registryMetadata.OfType<AssociationDto>().FirstOrDefault(assoc => assoc.TargetObject == documentEntry.Id);
        var submissionSet = registryMetadata.OfType<SubmissionSetDto>().FirstOrDefault(ss => ss.Id == association?.SourceObject);
        var document = documents.FirstOrDefault(doc => doc.DocumentId == documentEntry.UniqueId);

        var cdaDocument = CdaTransformer.TransformRegistryObjectsToClinicalDocument(documentEntry, submissionSet, document);
        var sxmls = new SoapXmlSerializer();
        var cdaXml = sxmls.SerializeSoapMessageToXmlString(cdaDocument).Content;
        var cdaDocumentAgain = sxmls.DeserializeXmlString<ClinicalDocument>(cdaXml);
    }

    [Fact]
    public async Task TransformCdaToRegistryObjects()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "ClinicalDocumentArchitecture"));

        foreach (var file in testDataFiles.Where(fl => fl.Contains("cda",StringComparison.CurrentCultureIgnoreCase)) ?? [])
        {
            var cdaXml = File.ReadAllText(file);

            var sxmls = new SoapXmlSerializer();

            var cdaDocument = sxmls.DeserializeXmlString<ClinicalDocument>(cdaXml);

            var documentReference = CdaTransformer.TransformClinicalDocumentToRegistryObjects(cdaDocument, "2.16.578.1.12.4.5.100.1", "2.16.578.1.12.4.5.100.1.2");

            var registryObjects = RegistryMetadataTransformer.TransformDocumentReferenceDtoToRegistryObjects(documentReference);
        }
    }
}