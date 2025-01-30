using System.Xml.Serialization;
using XcaGatewayService.Commons;
using XcaGatewayService.Commons.Enums;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryErrorList
{
    [XmlElement("RegistryError", Order = 0)]
    public RegistryErrorType[] RegistryError;

    [XmlAttribute(AttributeName = "highestSeverity", DataType = "anyURI")]
    public string HighestSeverity;
}
