using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

public class AlternateIdentification
{
    [XmlAttribute("classCode")]
    public string ClassCode { get; set; }

    [XmlElement("id")]
    public II Id { get; set; }

    [XmlElement("code")]
    public CD? Code { get; set; }

    [XmlElement("statusCode")]
    public CS? StatusCode { get; set; }

    [XmlElement("effectiveTime")]
    public IVL_TS? EffectiveTime { get; set; }
}