using System.Xml;
using System.Xml.Serialization;
using PdfSharp.Events;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.Tests;

public class UnitTests_ClinicalDocument
{
    [Fact]
    public async Task CDA_SerializeDeserialize()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        foreach (var file in testDataFiles)
        {
            if (!file.Contains("CDA")) continue;

            var fileContent = File.ReadAllText(file);

            var docc = await sxmls.DeserializeSoapMessageAsync<ClinicalDocument>(fileContent);

            var doccCDA = sxmls.SerializeSoapMessageToXmlString(docc);

            int file1 = fileContent.Split("\n").Length;
            int file2 = doccCDA.Content.Split("\n").Length;
            int diff = file1 - file2;
            Assert.InRange(diff,0,3);
        }
    }

    //[Fact]
    //public async Task TransformCdaToIti41()
    //{
    //    var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

    //    var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

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
        var registry = new FileBasedRegistry();
        var registryObjects = registry.ReadRegistry();


        var randomIndex = new Random().Next(registryObjects.OfType<DocumentEntryDto>().Count());

        var documentEntry = registryObjects.OfType<DocumentEntryDto>().ElementAt(randomIndex);

        var association = registryObjects.OfType<AssociationDto>().FirstOrDefault(assoc => assoc.TargetObject == documentEntry.Id);

        var submissionSet = registryObjects.OfType<SubmissionSetDto>().FirstOrDefault(ss => ss.Id == association?.SourceObject);

        var repository = new FileBasedRepository(new ApplicationConfig() { RepositoryUniqueId = documentEntry.RepositoryUniqueId, HomeCommunityId = documentEntry.HomeCommunityId });

        var document = new DocumentDto()
        {
            Data = repository.Read(documentEntry.Id),
            DocumentId = documentEntry.Id,
        };

        var cdaDocument = CdaTransformerService.TransformRegistryObjectsToClinicalDocument(documentEntry, submissionSet, document);
        var sxmls = new SoapXmlSerializer();
        var cdaXml = sxmls.SerializeSoapMessageToXmlString(cdaDocument).Content;
        var cdaDocumentAgain = await sxmls.DeserializeSoapMessageAsync<ClinicalDocument>(cdaXml);
    }
}