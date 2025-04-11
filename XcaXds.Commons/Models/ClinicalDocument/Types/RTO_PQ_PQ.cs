﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

public class RTO_PQ_PQ : QTY
{
    [XmlElement("numerator")]
    public PQ? Numerator { get; set; }
    
    [XmlElement("denominator")]
    public PQ? Denominator { get; set; }

}