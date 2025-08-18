using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CV : CE
{
    [XmlAttribute("codeSystem")]
    public string? CodeSystem { get; set; }

    [XmlAttribute("displayName")]
    public string? DisplayName { get; set; }
}
