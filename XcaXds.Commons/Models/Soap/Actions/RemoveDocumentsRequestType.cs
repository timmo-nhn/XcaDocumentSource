using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rmd)]
public class RemoveDocumentsRequestType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public DocumentRequestType[]? DocumentRequest { get; set; }
}
