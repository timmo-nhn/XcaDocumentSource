using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

public class BirthPlace
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("place")]
    public Place Place { get; set; }
}