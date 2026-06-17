using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType("ExternalIdentifier", Namespace = Constants.Xds.Namespaces.Rim)]
public class ExternalIdentifierType : RegistryObjectType
{
    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "registryObject", DataType = "anyURI")]
    public string? RegistryObject;

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "identificationScheme", DataType = "anyURI")]
    public string? IdentificationScheme;

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "value")]
    public string? Value;
}
