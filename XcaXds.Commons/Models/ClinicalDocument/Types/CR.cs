using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CR
{
    [XmlIgnore]
    public bool? Inverted { get; set; }

    [XmlAttribute("inverted")]
    public string? InvertedAsString
    {
        get => Inverted.HasValue ? Inverted.Value.ToString().ToLower() : null;
        set => Inverted = string.IsNullOrEmpty(value) ? null : bool.Parse(value);
    }

    [XmlElement("name")]
    public CV? Name { get; set; }

    [XmlElement("value")]
    public CD? Value { get; set; }
}
