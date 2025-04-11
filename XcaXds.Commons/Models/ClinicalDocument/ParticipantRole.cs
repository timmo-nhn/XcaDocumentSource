﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("participantRole", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ParticipantRole
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("sdtcIdentifiedBy", Namespace=Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<IdentifiedBy>? SdtcIdentifiedBy { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("addr")]
    public List<AD>? Address { get; set; }

    [XmlElement("telecom")]
    public List<TEL>? Telecom { get; set; }

    [XmlElement("playingDevice")]
    public Device? ScopingEntity { get; set; }

    [XmlElement("playingEntity")]
    public PlayingEntity? PlayingEntity { get; set; }

    [XmlElement("scopingEntity")]
    public Entity? PlayingDevice { get; set; }
}