﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class IVL_TS : SXCM_TS
{
    [XmlElement("low")]
    public IVXB_TS? Low { get; set; }

    [XmlElement("center")]
    public TS? Center { get; set; }

    [XmlElement("width")]
    public PQ Width { get; set; }

    [XmlElement("high")]
    public IVXB_TS? High { get; set; }

}