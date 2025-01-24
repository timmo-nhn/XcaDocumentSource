using System.Xml.Serialization;
using XcaDocumentSource.Models.Soap.XdsTypes;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

[XmlInclude(typeof(RegistryObjectType))]
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
[XmlInclude(typeof(ObjectRefType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class IdentifiableType
{

    [XmlElement("Slot", Order = 0)]
    public SlotType[] Slot;


    [XmlAttribute(AttributeName = "id", DataType = "anyURI")]
    public string Id;


    [XmlAttribute(AttributeName = "home", DataType = "anyURI")]
    public string Home;
}
