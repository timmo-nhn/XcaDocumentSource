using System.ComponentModel;
using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Query)]
public partial class AdhocQueryResponse : RegistryResponseType
{
    public AdhocQueryResponse()
    {
        StartIndex = "0";
    }

    public IdentifiableType[]? RegistryObjectList { get; set; }

    [XmlAttribute(AttributeName = "startIndex", DataType = "integer")]
    [DefaultValue("0")]
    public string StartIndex = "0";

    [XmlAttribute(AttributeName = "totalResultCount", DataType = "integer")]
    public string? TotalResultCount;
}
