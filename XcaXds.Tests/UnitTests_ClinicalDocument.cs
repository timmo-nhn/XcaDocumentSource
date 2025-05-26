using XcaXds.Commons.Models.ClinicalDocumentArchitecture;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

namespace XcaXds.Tests;

public class UnitTests_ClinicalDocument
{
    private readonly CdaTransformerService _transformerService;
    public UnitTests_ClinicalDocument()
    {
        _transformerService = new CdaTransformerService();
    }

    [Fact]
    public async Task SerializeDeserialize()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        foreach (var file in testDataFiles)
        {
            var fileContent = string.Empty;

            if (file.Contains("CDA"))
            {
                fileContent = File.ReadAllText(file);
            }
            else continue;

            var docc = await sxmls.DeserializeSoapMessageAsync<ClinicalDocument>(fileContent);


            Console.WriteLine(docc.Code);

            var doccCDA = sxmls.SerializeSoapMessageToXmlString(docc);

            int file1 = fileContent.Split("\n").Length;
            int file2 = doccCDA.Content.Split("\n").Length;
            int diff = file1 - file2;
            Assert.Equal(diff, 0);
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
    public async Task TransformIti41ToCda()
    {

        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        foreach (var file in testDataFiles)
        {
            var fileContent = string.Empty;

            if (file.Contains("PnR_request"))
            {
                using var reader = File.OpenText(file);
                fileContent = await reader.ReadToEndAsync();
            }
            else continue;

            var docc = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(fileContent);

            try
            {
                var cdaDocument = _transformerService.TransformProvideAndRegisterRequestToClinicalDocument(docc.Body.ProvideAndRegisterDocumentSetRequest.SubmitObjectsRequest.RegistryObjectList, docc.Body.ProvideAndRegisterDocumentSetRequest.Document);
                var gobb = sxmls.SerializeSoapMessageToXmlString(cdaDocument);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}