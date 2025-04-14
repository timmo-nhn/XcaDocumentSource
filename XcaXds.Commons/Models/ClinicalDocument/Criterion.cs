using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("criterion", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Criterion
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("moodCode")]
    public string? MoodCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("code")]
    public CD? Code { get; set; }

    [XmlElement("text")]
    public ED? Text { get; set; }

    [XmlElement("value", typeof(ANY))]
    public ANY? Value { get; set; }
}
