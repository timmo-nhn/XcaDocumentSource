﻿using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class PQR : CV
{
    [XmlAttribute("value")]
    public string? Value { get; set; }
}
