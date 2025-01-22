using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class PersonNameType
{
    [XmlAttribute(AttributeName = "firstName")]
    public string FirstName;

    [XmlAttribute(AttributeName = "middleName")]
    public string MiddleName;

    [XmlAttribute(AttributeName = "lastName")]
    public string LastName;
}
