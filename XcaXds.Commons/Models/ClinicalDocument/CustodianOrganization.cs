﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;
[Serializable]
[XmlType("custodianOrganization", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CustodianOrganization
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II> Id { get; set; }

    [XmlElement("name")]
    public ON? Name { get; set; }

    [XmlElement("telecom")]
    public TEL? Telecom { get; set; }

    [XmlElement("addr")]
    public AD? Address { get; set; }
}