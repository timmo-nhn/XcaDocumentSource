﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("manufacturedProduct", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ManufacturedProduct
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public List<II>? Id { get; set; }

    [XmlElement("identifiedBy", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public List<IdentifiedBy> SdtcIdentifiedBy { get; set; }

    [XmlElement("manufacturedLabeledDrug")]
    public LabeledDrug? ManufacturedLabeledDrug { get; set; }

    [XmlElement("manufacturedMaterial")]
    public Material? ManufacturedMaterial { get; set; }

    [XmlElement("manufacturerOrganization")]
    public Organization? ManufacturerOrganization { get; set; }

}