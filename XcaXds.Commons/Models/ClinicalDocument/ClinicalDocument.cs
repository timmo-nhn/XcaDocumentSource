using System.Globalization;
using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[XmlRoot(Namespace = Constants.Xds.Namespaces.Hl7V3)]
[Serializable]
public class ClinicalDocument
{
    [XmlAttribute("classCode")]
    public string classCode { get; set; }

    [XmlAttribute("moodCode")]
    public string moodCode { get; set; }

    [XmlElement("realmCode")]
    public CS RealmCode { get; set; }

    [XmlElement("typeId")]
    public II TypeId { get; set; }

    [XmlElement("templateId")]
    public II TemplateId { get; set; }

    [XmlElement("id")]
    public II Id { get; set; }

    [XmlElement("code")]
    public CV Code { get; set; }

    [XmlElement("title")]
    public string Title { get; set; }

    [XmlElement("effectiveTime")]
    public TS EffectiveTime { get; set; }

    [XmlElement("confidentialityCode")]
    public CV ConfidentialityCode { get; set; }

    [XmlElement("languageCode")]
    public CS LanguageCode { get; set; }

    [XmlElement("setId")]
    public II SetId { get; set; }

    [XmlElement("recordTarget")]
    public RecordTarget RecordTarget { get; set; }

}