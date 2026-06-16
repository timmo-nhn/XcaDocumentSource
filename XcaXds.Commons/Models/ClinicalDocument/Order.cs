using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("order", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Order
{
    [XmlAttribute("classCode")]
    public string classCode { get; set; } = string.Empty;

    [XmlAttribute("moodCode")]
    public string? moodCode { get; set; } = "RQO";

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II> Id { get; set; } = new();

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("prioritycode")]
    public CE? PriorityCode { get; set; }

}