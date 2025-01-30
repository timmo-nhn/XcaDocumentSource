using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[XmlInclude(typeof(NotificationType))]
[XmlInclude(typeof(AdhocQueryType))]
[XmlInclude(typeof(SubscriptionType))]
[XmlInclude(typeof(FederationType))]
[XmlInclude(typeof(RegistryType))]
[XmlInclude(typeof(PersonType))]
[XmlInclude(typeof(UserType))]
[XmlInclude(typeof(SpecificationLinkType))]
[XmlInclude(typeof(ServiceBindingType))]
[XmlInclude(typeof(ServiceType))]
[XmlInclude(typeof(RegistryPackageType))]
[XmlInclude(typeof(OrganizationType))]
[XmlInclude(typeof(ExtrinsicObjectType))]
[XmlInclude(typeof(ExternalLinkType))]
[XmlInclude(typeof(ClassificationSchemeType))]
[XmlInclude(typeof(ClassificationNodeType))]
[XmlInclude(typeof(AuditableEventType))]
[XmlInclude(typeof(AssociationType))]
[XmlInclude(typeof(ExternalIdentifierType))]
[XmlInclude(typeof(ClassificationType))]
//[XmlInclude(typeof(ProvideAndRegisterDocumentSetbRequest))]

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class RegistryObjectType : IdentifiableType
{
    [XmlElement(Order = 0)]
    public InternationalStringType Name;

    [XmlElement(Order = 1)]
    public InternationalStringType Description;

    [XmlElement(Order = 2)]
    public VersionInfoType VersionInfo;

    [XmlElement("Classification", Order = 3)]
    public ClassificationType[] Classification;

    [XmlElement("ExternalIdentifier", Order = 4)]
    public ExternalIdentifierType[] ExternalIdentifier;

    [XmlAttribute(AttributeName = "lid", DataType = "anyURI")]
    public string Lid;

    [XmlAttribute(AttributeName = "objectType", DataType = "anyURI")]
    public string ObjectType;

    [XmlAttribute(AttributeName = "status", DataType = "anyURI")]
    public string Status;
}
