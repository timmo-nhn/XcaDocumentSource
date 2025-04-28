
using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

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