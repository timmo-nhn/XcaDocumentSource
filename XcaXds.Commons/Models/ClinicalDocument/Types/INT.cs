using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class INT
{
    [XmlIgnore]
    public int? Value { get; set; }

    [XmlAttribute("value")]
    public string? ValueAsString
    {
        get => Value.HasValue ? Value.Value.ToString() : null;
        set => Value = string.IsNullOrEmpty(value) ? null : int.Parse(value);
    }
}


