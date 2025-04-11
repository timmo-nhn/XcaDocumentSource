using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

public class ANY
{
    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }
}