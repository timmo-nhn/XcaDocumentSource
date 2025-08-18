using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[XmlInclude(typeof(NotifyActionType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public abstract class ActionType
{
}
