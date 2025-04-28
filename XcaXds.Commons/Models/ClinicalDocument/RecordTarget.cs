using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("recordTarget", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class RecordTarget
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; } = "RCT";

    [XmlAttribute("contextControlCode")]
    public string? ContextControlCode { get; set; } = "OP";

    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II? TypeId { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("patientRole")]
    public PatientRole PatientRole { get; set; }
}
