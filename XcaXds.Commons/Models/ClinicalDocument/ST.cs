using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ST : ANY
{
    [XmlAttribute("representation")]
    public string? Representation { get; set; }

    [XmlAttribute("mediaType")]
    public string? MediaType { get; set; }

    [XmlAttribute("language")]
    public string? Language { get; set; }

    [XmlText]
    public string? Value { get; set; }
}