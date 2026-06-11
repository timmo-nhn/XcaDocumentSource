using System.Xml.Serialization;
using XcaXds.Shared.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class RemoveObjectsRequestType
{
    [XmlElement("ObjectRefList")]
    public ObjectRefList? ObjectRefList { get; set; }
}

