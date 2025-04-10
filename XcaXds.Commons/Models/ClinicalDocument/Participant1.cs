using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("participant", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Participant1
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlAttribute("contextControlCode")]
    public string? ContextControlCode { get; set; } = "OP";

    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II? TypeId { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("functionCode")]
    public CE? FunctionCode { get; set; }

    [XmlElement("time")]
    public IVL_TS? Time { get; set; }

    [XmlElement("associatedEntity")]
    public AssociatedEntity AssociatedEntity { get; set; }
}