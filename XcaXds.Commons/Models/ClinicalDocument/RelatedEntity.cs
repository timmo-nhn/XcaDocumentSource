using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

public class RelatedEntity
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("addr")]
    public AD Address { get; set; }

    [XmlElement("telecom")]
    public List<TEL>? Telecom { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS? EffectiveTime { get; set; }

    [XmlElement("relatedPerson")]
    public Person? RelatedPerson { get; set; }

}