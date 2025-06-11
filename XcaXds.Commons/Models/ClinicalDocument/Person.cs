using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("person", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Person
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("name")]
    public List<PN>? Name { get; set; }

    [XmlElement("asPatientRelationship", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<CE>? SdtcAsPatientRelationShip { get; set; }
}