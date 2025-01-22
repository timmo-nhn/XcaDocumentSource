using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ExternalIdentifierType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "registryObject", DataType = "anyURI")]
    public string RegistryObject;

    [XmlAttribute(AttributeName = "identificationScheme", DataType = "anyURI")]
    public string IdentificationScheme;

    [XmlAttribute(AttributeName = "value")]
    public string Value;
}
