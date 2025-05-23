﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

public class Authenticator
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II? TypeId { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("time")]
    public TS Time { get; set; }

    [XmlElement("signatureCode")]
    public CS SignatureCode { get; set; }

    [XmlElement("signatureText", Namespace = Constants.Hl7.Namespaces.Hl7Sdtc)]
    public ED? SdtcSignatureText { get; set; }

    [XmlElement("assignedEntity")]
    public AssignedEntity AssignedEntity { get; set; }
}