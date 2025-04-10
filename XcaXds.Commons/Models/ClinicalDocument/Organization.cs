﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("organization", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Organization
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "ORG";

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; } = "INSTANCE";

    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II? TypeId { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId{ get; set; }

    [XmlElement("id")]
    public List<II>? Id { get; set; }

    [XmlElement("name")]
    public List<ON>? Name { get; set; }

    [XmlElement("telecom")]
    public List<TEL>? Telecom {  get; set; }

    [XmlElement("addr")]
    public List<AD>? Address {  get; set; }

    [XmlElement("standardIndustryClassCode")]
    public CE? StandardIndustryClassCode { get; set; }

    [XmlElement("asOrganizationPartOf")]
    public OrganizationPartOf? OrganizationPartOf { get; set; }
}