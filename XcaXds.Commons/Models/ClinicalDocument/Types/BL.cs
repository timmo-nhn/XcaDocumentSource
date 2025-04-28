using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class BL : ANY
{
    [XmlIgnore]
    private bool? _value;

    [XmlAttribute("value")]
    public string? Value
    {
        get => _value.HasValue ? _value.ToString().ToLowerInvariant() : null;
        set => _value = string.IsNullOrEmpty(value) ? null : bool.Parse(value);
    }

}
