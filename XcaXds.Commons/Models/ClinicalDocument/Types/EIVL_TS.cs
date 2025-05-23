﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class EIVL_TS : SXCM_TS
{
    [XmlElement("event")]
    public CE? Event { get; set; }

    [XmlElement("offset")]
    public IVL_PQ? Offset { get; set; }
}