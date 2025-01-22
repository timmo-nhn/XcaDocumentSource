using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ExternalLinkType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "externalURI", DataType = "anyURI")]
    public string ExternalUri;
}
