using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class MO : QTY
{
    [XmlElement("currency")]
    public CS? Currency { get; set; }

    [XmlText]
    public int Value { get; set; }
}
