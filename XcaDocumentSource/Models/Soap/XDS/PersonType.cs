using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[XmlInclude(typeof(UserType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class PersonType : RegistryObjectType
{
    [XmlElement("Address", Order = 0)]
    public PostalAddressType[] Address;

    [XmlElement(Order = 1)]
    public PersonNameType PersonName;

    [XmlElement("TelephoneNumber", Order = 2)]
    public TelephoneNumberType[] TelephoneNumber;

    [XmlElement("EmailAddress", Order = 3)]
    public EmailAddressType[] EmailAddress;
}
