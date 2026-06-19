using System.Xml.Serialization;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class II : ANY
{
    [XmlAttribute("root")]
    public string Root { get; set; } = string.Empty;

    [XmlAttribute("extension")]
    public string Extension { get; set; } = string.Empty;
}
