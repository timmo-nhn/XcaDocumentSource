using System.Xml.Serialization;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class CS : CV
{
}
