using System.Xml.Serialization;

namespace XcaXds.Commons.Models;

[Serializable]
[XmlRoot("Include", Namespace = Constants.Soap.Namespaces.XopInclude)]
public partial class IncludeType
{
    [XmlAttribute()]
    public string href { get; set; }
}
