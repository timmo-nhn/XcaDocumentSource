using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryErrorList
{
    [XmlElement("RegistryError", Order = 0)]
    public RegistryErrorType[] RegistryError;

    [XmlAttribute(AttributeName = "highestSeverity", DataType = "anyURI")]
    public string HighestSeverity;

}
