using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ENXP : ST
{
    [XmlText]
    public string? Value { get; set; }

    [XmlAttribute("partType")]
    public string? PartType { get; set; }

    [XmlAttribute("qualifier")]
    public string? QualifierRaw { get; set; }

    [XmlIgnore]
    public List<string> Qualifier
    {
        get => QualifierRaw?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new();
        set => QualifierRaw = string.Join(" ", value);
    }
}
