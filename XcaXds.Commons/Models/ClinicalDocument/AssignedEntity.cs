using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("assignedEntity", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class AssignedEntity
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II> Id { get; set; }

    [XmlElement("identifiedBy", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<IdentifiedBy>? SdtcIdentifiedBy { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("addr")]
    public List<AD>? Address { get; set; }

    [XmlElement("telecom")]
    public List<TEL>? Telecom { get; set; }

    [XmlElement("assignedPerson")]
    public Person? AssignedPerson { get; set; }

    [XmlElement("representedOrganization")]
    public Organization? Organization { get; set; }

}