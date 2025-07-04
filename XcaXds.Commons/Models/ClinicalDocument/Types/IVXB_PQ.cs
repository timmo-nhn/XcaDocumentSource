﻿
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class IVXB_PQ : PQ
{
    [XmlIgnore]
    private bool? _inclusive;

    [XmlAttribute("inclusive")]
    public string? Inclusive
    {
        get => _inclusive.HasValue ? _inclusive.ToString().ToLowerInvariant() : null;
        set => _inclusive = string.IsNullOrEmpty(value) ? null : bool.Parse(value);
    }
}