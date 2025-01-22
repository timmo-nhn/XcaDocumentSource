using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class EmailAddressType
{
    [XmlAttribute(AttributeName = "address")]
    public string Address;

    [XmlAttribute(AttributeName = "type")]
    public string Type;
}
