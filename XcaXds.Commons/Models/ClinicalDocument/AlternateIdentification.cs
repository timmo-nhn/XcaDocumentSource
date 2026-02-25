using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

public class AlternateIdentification
{
    [XmlAttribute("classCode")]
    public string ClassCode { get; set; } = "IDENT";

    [XmlElement("id")]
    public II Id { get; set; } = new II();

    [XmlElement("code")]
    public CD? Code { get; set; }

    [XmlElement("statusCode")]
    public CS? StatusCode { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS? EffectiveTime { get; set; }
}