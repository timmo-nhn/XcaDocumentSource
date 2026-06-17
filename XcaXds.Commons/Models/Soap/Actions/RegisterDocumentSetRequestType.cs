using System.Xml.Serialization;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Xdsb)]
public class RegisterDocumentSetRequestType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Lcm)]
    public SubmitObjectsRequest? SubmitObjectsRequest { get; set; }
}
