using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Svs)]
public class ConceptType
{
    [XmlAttribute(AttributeName = "code")]
    public string code;

    [XmlAttribute(AttributeName = "codeSystemName")]
    public string codeSystemName;

    [XmlAttribute(AttributeName = "displayName")]
    public string displayName;
}