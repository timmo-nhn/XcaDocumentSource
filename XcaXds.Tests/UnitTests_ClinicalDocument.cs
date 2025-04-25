using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Services;

namespace XcaXds.Tests;

public class UnitTests_ClinicalDocument
{
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
}