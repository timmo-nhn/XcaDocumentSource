using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class II : ANY
{
    [XmlAttribute("root")]
    public string Root { get; set; }

    [XmlAttribute("extension")]
    public string Extension { get; set; }
}
