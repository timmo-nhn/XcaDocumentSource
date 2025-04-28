using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("maintainedEntity", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class MaintainedEntity
{
    [XmlElement("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS EffectiveTime { get; set; }

    [XmlElement("maintainingPerson")]
    public Person MaintainingPerson { get; set; }
}