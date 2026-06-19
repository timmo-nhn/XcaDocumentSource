using System.Xml.Serialization;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class AdhocQueryType : RegistryObjectType
{
}
