using System.ComponentModel;
using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("encompassingEncounter", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class EncompassingEncounter
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "ENC";

    [XmlAttribute("moodCode")]
    public string? MoodCode { get; set; } = "EVN";

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II>? Id { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS? EffectiveTime { get; set; }

    [XmlElement("sdtcAdmissionReferralSourceCode", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public CE? SdtcAdmissionReferralSourceCode { get; set; }

    [XmlElement("dischargeDispositionCode")]
    public CE? DischargeDispositionCode { get; set; }

    [XmlElement("responsibleParty")]
    public InfrastructureRoot? ResponsibleParty { get; set; }

    [XmlElement("encounteredParticipant")]
    public List<EncounterParticipant> EncounterParticipant { get; set; }

    [XmlElement("location")]
    public Location? Location { get; set; }

}