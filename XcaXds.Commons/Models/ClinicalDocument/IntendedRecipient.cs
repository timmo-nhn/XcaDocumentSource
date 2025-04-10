using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;


[Serializable]
[XmlType("intendedRecipient", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class IntendedRecipient
{
    [XmlAttribute("classCode")]
    public string classCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II> Id { get; set; }

    [XmlElement("sdtcIdentifiedBy", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<IdentifiedBy>? SdtcIdentifiedBy { get; set; }

    [XmlElement("addr")]
    public List<AD>? Address { get; set; }

    [XmlElement("telecom")]
    public List<TEL>? Telecom { get; set; }

    [XmlElement("informationRecipient")]
    public Person? InformationRecipient { get; set; }

    [XmlElement("receivedOrganization")]
    public Organization? ReceivedOrganization { get; set; }
}