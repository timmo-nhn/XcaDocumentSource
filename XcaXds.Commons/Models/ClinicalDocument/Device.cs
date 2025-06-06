﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("device", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Device
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("manufacturerModelName")]
    public SC? ManufacturerModelName { get; set; }

    [XmlElement("softwareName")]
    public SC? SoftwareName { get; set; }

}