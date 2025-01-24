using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

[XmlInclude(typeof(NotifyActionType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public abstract class ActionType
{
}
