using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[XmlInclude(typeof(NotifyActionType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public abstract class ActionType
{
}
