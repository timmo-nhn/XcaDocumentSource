using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rmd)]
public class RemoveDocumentsRequestType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public DocumentRequestType[] DocumentRequest { get; set; }
}
