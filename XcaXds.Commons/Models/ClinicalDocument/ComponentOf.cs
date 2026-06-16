using System.Xml.Serialization;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("componentOf", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ComponentOf : InfrastructureRoot
{
    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlElement("encompassingEncounter")]
    public EncompassingEncounter EncompassingEncounter { get; set; } = new();
}

