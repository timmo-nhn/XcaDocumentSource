using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class IVXB_TS
{
    [XmlAttribute("inclusive")]
    public bool Inclusive { get; set; } = false;
}