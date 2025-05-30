﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("reference", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Reference
{
    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlElement("seperatableInd")]
    public BL? SeperatableInd { get; set; }

    [XmlElement("externalAct", typeof(ExternalAct))]
    [XmlElement("externalObservation", typeof(ExternalObservation))]
    [XmlElement("externalProcedure", typeof(ExternalProcedure))]
    [XmlElement("externalDocument", typeof(ExternalDocument))]
    public ExternalBase ExternalItem { get; set; }
}