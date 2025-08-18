using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("labeledDrug", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class LabeledDrug
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("name")]
    public EN? Name { get; set; }
}