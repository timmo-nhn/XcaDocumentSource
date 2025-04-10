using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("person", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Person
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "PSN";

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; } = "INSTANCE";

    [XmlElement("templateId")]
    public List<II>? TemplateId {  get; set; }

    [XmlElement("name")]
    public List<PN>? Name { get; set; }

    [XmlElement("sdtcAsPatientRelationhip",Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<CE>? SdtcAsPatientRelationShip { get; set; }
}