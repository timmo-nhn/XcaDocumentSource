using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("consent", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Consent
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("moodCode")]
    public string? MoodCodeT { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II>? Id { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("statusCode")]
    public CS StatusCode { get; set; }

}