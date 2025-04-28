using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CR : ANY
{
    [XmlIgnore]
    private bool? _inverted { get; set; }

    [XmlAttribute("inverted")]
    public string? Inverted
    {
        get => _inverted.HasValue ? _inverted.Value.ToString().ToLowerInvariant() : null;
        set => _inverted = string.IsNullOrEmpty(value) ? null : bool.Parse(value);
    }

    [XmlElement("name")]
    public CV? Name { get; set; }

    [XmlElement("value")]
    public CD? Value { get; set; }
}
