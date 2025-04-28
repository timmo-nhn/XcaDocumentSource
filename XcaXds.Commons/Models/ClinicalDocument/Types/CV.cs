using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CV : CE
{
    [XmlAttribute("codeSystem")]
    public string? CodeSystem { get; set; }

    [XmlAttribute("displayName")]
    public string? DisplayName { get; set; }
}
