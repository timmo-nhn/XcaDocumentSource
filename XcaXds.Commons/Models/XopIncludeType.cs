using System.Xml.Serialization;
using XcaXds.Shared;

namespace XcaXds.Commons.Models;

[Serializable]
[XmlRoot("Include", Namespace = Constants.Soap.Namespaces.XopInclude)]
public partial class IncludeType
{
    [XmlAttribute()]
    public string? href { get; set; }
}
