using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class AdhocQueryType : RegistryObjectType
{
    [XmlElement(Order = 0)]
    public QueryExpressionType? QueryExpression;
}
