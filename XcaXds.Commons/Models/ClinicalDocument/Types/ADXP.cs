using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ADXP : ST
{
    [XmlAttribute("partType")]
    public string? PartType { get; set; }
}