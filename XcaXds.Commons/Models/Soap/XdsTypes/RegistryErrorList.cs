using System.Xml.Serialization;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryErrorList
{
    public RegistryErrorList()
    {
        RegistryError = [];
    }

    [XmlElement("RegistryError")]
    public RegistryErrorType[] RegistryError { get; set; }

    [XmlAttribute(AttributeName = "highestSeverity", DataType = "anyURI")]
    public string? HighestSeverity { get; set; }
}
