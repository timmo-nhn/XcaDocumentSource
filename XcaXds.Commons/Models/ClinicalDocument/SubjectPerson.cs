using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("subjectPerson", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class SubjectPerson
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "PSN";

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; } = "INSTANCE";

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("name")]
    public List<PN>? Name { get; set; }

    [XmlElement("sdtcDesc", Namespace =Constants.Hl7.Namespaces.Hl7Sdtc)]
    public ED? SdtcDesc { get; set; }

    [XmlElement("administrativeGenderCode")]
    public CE? AdministrativeGenderCode { get; set; }

    [XmlElement("birthTime")]
    public TS? BirthTime { get; set; }

    [XmlElement("sdtcDeceasedInd")]
    public BL? SdtcDeceasedInd { get; set; }

    [XmlElement("sdtcDeceasedTime")]
    public TS? SdtcDeceasedTime { get; set; }
}