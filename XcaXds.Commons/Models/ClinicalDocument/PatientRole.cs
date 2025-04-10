﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;
[Serializable]
[XmlType("patientRole", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class PatientRole
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "PAT";

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

    [XmlElement("patient")]
    public Patient? Patient { get; set; }

    [XmlElement("providerOrganization")]
    public Organization? ProviderOrganization { get; set; }

}
