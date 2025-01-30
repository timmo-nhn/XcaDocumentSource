using System.Xml;
using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class QueryExpressionType
{
    [XmlText]
    [XmlAnyElement(Order = 0)]
    public XmlNode[] Any;


    [XmlAttribute(AttributeName = "queryLanguage", DataType = "anyURI")]
    public string QueryLanguage;
}
