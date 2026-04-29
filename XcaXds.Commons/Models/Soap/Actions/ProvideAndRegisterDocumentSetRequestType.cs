using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Xdsb)]
public class ProvideAndRegisterDocumentSetRequestType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Lcm)]
    public SubmitObjectsRequest? SubmitObjectsRequest { get; set; }

    [XmlElement("Document")]
    public DocumentType[]? Document { get; set; }
}
