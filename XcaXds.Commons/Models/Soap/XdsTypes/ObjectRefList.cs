using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ObjectRefList
{
    [XmlElement(Order = 0)]
    public IdentifiableType[]? ObjectRef;
}


