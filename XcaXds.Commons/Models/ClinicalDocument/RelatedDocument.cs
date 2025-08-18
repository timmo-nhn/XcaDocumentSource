using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlRoot("relatedDocument", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class RelatedDocument
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II? TypeId { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("parentDocument")]
    public ParentDocument ParentDocument { get; set; }
}