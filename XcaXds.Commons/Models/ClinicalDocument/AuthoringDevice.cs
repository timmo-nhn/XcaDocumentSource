
using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("authoringDevice", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class AuthoringDevice
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; } = "DEV";

    [XmlAttribute("determinerCode")]
    public string? DeterminerCode { get; set; } = "INSTANCE";


    [XmlElement("manufacturerModelName")]
    public SC? ManufacturerModelName { get; set; }

    [XmlElement("softwareName")]
    public SC? SoftwareName { get; set; }

    [XmlElement("asMaintainedEntity")]
    public List<MaintainedEntity>? AsMaintainedEntity { get; set; }
}