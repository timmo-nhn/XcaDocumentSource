﻿

using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("subject", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Subject : InfrastructureRoot
{
    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlAttribute("contextControlCode")]
    public string? ContextControlCode { get; set; }

    [XmlElement("awarenessCode")]
    public CE? AwarenessCode { get; set; }

    [XmlElement("relatedSubject")]
    public RelatedSubject RelatedSubject { get; set; }
}