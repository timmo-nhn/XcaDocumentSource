﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class PQ : QTY
{
    [XmlAttribute("unit")]
    public string Unit { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlElement("translation")]
    public List<PQR>? Translation { get; set; }
}