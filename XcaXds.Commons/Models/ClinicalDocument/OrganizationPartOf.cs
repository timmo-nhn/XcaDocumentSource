﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("organizationPartOf", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class OrganizationPartOf
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II>? Id { get; set; }

    [XmlElement("identifiedBy", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<IdentifiedBy>? SdtcIdentifiedBy { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("statusCode")]
    public CS? StatusCode { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS? EffectiveTime { get; set; }

    [XmlElement("wholeOrganization")]
    public Organization? WholeOrganization { get; set; }

}