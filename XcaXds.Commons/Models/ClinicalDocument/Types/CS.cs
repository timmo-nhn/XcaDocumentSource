﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CS
{
    [XmlAttribute("code")]
    public string? Code { get; set; }
}
