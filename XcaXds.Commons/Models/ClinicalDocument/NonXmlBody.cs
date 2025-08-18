using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("nonXmlBody", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class NonXmlBody
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("moodCode")]
    public string? MoodCode { get; set; }

    [XmlElement("text")]
    public ED Text { get; set; }

    [XmlElement("confidentialityCode")]
    public CE? ConfidentialityCode { get; set; }

    [XmlElement("languageCode")]
    public CS? LanguageCode { get; set; }
}