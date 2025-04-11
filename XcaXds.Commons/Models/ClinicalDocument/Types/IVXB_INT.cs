using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class IVXB_INT : INT
{
    [XmlIgnore]
    private bool? _inclusive;

    [XmlIgnore]
    public bool? Inclusive
    {
        get => _inclusive;
        set => _inclusive = value;
    }

    [XmlAttribute("inclusive")]
    public string InclusiveAsString
    {
        get => _inclusive.HasValue ? _inclusive.ToString().ToLowerInvariant() : null;
        set => _inclusive = string.IsNullOrEmpty(value) ? (bool?)null : bool.Parse(value);
    }
}
