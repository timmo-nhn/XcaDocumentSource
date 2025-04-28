using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ADXP : ST
{
    [XmlAttribute("partType")]
    public string? PartType { get; set; }

    [XmlText]
    public string? Value { get; set; }

}