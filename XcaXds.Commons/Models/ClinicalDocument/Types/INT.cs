using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class INT : QTY
{
    [XmlIgnore]
    public int? Value { get; set; }

    [XmlAttribute("value")]
    public string? ValueAsString
    {
        get => Value.HasValue ? Value.ToString().ToLowerInvariant() : null;
        set => Value = string.IsNullOrEmpty(value) ? null : int.Parse(value);
    }
}


