using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class OrganizationType : RegistryObjectType
{
    [XmlElement("Address", Order = 0)]
    public PostalAddressType[] Address;

    [XmlElement("TelephoneNumber", Order = 1)]
    public TelephoneNumberType[] TelephoneNumber;

    [XmlElement("EmailAddress", Order = 2)]
    public EmailAddressType[] EmailAddress;

    [XmlAttribute(AttributeName = "parent", DataType = "anyURI")]
    public string Parent;

    [XmlAttribute(AttributeName = "primaryContact", DataType = "anyURI")]
    public string PrimaryContact;
}
