using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("maintainedEntity", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class MaintainedEntity
{
    [XmlElement("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS EffectiveTime { get; set; } = new();

    [XmlElement("maintainingPerson")]
    public Person MaintainingPerson { get; set; } = new();
}