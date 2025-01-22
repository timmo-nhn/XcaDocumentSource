using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class UserType : PersonType
{
}
