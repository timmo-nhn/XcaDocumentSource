using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("material", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Material
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "MMAT";

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; } = "KIND";

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("name")]
    public EN? Name { get; set; }

    [XmlElement("lotNumberText")]
    public ST? LotNumberText { get; set; }

}