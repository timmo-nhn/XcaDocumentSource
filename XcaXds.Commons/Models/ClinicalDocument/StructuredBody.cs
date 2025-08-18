
using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("structuredBody", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class StructuredBody : InfrastructureRoot
{
    [XmlAttribute("classCode")]
    public string? classCode { get; set; }

    [XmlAttribute("moodCode")]
    public string? moodCode { get; set; }

    [XmlElement("confidentialityCode")]
    public CE? ConfidentialityCode { get; set; }

    [XmlElement("languageCode")]
    public CS? LanguageCode { get; set; }

    [XmlElement("component")]
    public List<Component>? Component { get; set; }
}