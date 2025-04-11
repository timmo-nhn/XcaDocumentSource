using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("place", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Place
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "PLC";

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; } = "INSTANCE";

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("name")]
    public EN? Name { get; set; }

    [XmlElement("addr")]
    public AD? Address { get; set; }
}