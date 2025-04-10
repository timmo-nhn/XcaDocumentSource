using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class II
{
    [XmlAttribute("root")]
    public string Root { get; set; } = string.Empty;

    [XmlAttribute("extension")]
    public string Extension { get; set; } = string.Empty;
}
