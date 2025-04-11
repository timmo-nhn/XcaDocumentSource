using System.Xml;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ED : ANY
{
    [XmlAttribute("charset")]
    public string? Charset { get; set; }

    [XmlAttribute("compression")]
    public string? Compression { get; set; }

    [XmlAttribute("integrityCheck")]
    public byte[]? IntegrityCheck { get; set; }

    [XmlAttribute("integrityCheckAlgorithm")]
    public string? IntegrityCheckAlgorithm { get; set; }

    [XmlAttribute("language")]
    public string? Language { get; set; }

    [XmlAttribute("mediaType")]
    public string? MediaType { get; set; }

    [XmlAttribute("representation")]
    public string? Representation { get; set; }

    [XmlIgnore]
    private string? _gobb;

    [XmlText]
    public string? Data 
    { 
        get => _gobb; 
        set => _gobb = value; 
    }

    [XmlElement("reference")]
    public TEL? Reference { get; set; }

    [XmlElement("thumbnail")]
    public ED? Thumbnail { get; set; }
}
