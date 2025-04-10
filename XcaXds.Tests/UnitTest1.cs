namespace XcaXds.Tests;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Services;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        string cdaTestString = """
<ClinicalDocument classCode="DOCCLIN" moodCode="EVN" xmlns="urn:hl7-org:v3" xmlns:voc="urn:hl7-org:v3/voc">
    <realmCode code="NO" />
    <typeId extension="POCD_HD000040" root="2.16.840.1.113883.1.3" />
    <templateId root="2.16.578.1.34.10.123" />
    <id extension="urn:uuid:75c47112-278a-8db6-e063-2a20b40a304d" root="2.16.578.1.12.4.3.1.1.20.2" />
    <code code="C04-2" displayName="Patologi, histologi og cytologi" codeSystem="2.16.578.1.12.4.1.1.9602" />
    <title>Biopsi - NM 24 00001 - skjelettmuskelfiber UNS - Endelig svar</title>
    <effectiveTime value="20240228141302" />
    <confidentialityCode code="N" codeSystem="2.16.840.1.113883.5.25" />
    <languageCode code="no-NO" />
    <setId extension="364853685" root="2.16.578.1.12.4.3.1.1.20.2.15.1001" />
</ClinicalDocument>
""";

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var docc = await sxmls.DeserializeSoapMessageAsync<ClinicalDocument>(cdaTestString);

        Console.WriteLine(docc.Code);
    }
}