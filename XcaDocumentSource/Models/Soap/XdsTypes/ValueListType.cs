using System;
using System.ComponentModel;
using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class ValueListType
{
    [XmlElement("Value", Order = 0)]
    public string[] Value;

}
