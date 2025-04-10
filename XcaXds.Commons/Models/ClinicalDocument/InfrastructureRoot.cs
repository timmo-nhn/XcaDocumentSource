using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("infrastructureRoot", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class InfrastructureRoot
{
    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II? TypeId { get; set; }

    [XmlElement("templateId")]
    public List<II> TemplateId { get; set; }
}

[Serializable]
[XmlType("responsibleParty", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ResponsibleParty : InfrastructureRoot
{
    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; } = "RESP";

    [XmlElement("assignedEntity")]
    public AssignedEntity AssignedEntity { get; set; } = new();
}

[Serializable]
[XmlType("location", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Location : InfrastructureRoot
{
    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; } = "LOC";

    [XmlElement("healthcareFacility")]
    public HealthcareFacility HealthcareFacility { get; set; } = new();

}
