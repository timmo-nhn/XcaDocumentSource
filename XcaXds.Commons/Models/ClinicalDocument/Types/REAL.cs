﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class REAL : QTY
{
    [XmlAttribute("value")]
    public string? Value { get; set; }
}
