using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

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
    public SlotType[]? Slot;


    [XmlAttribute(AttributeName = "id", DataType = "anyURI")]
    public string Id;


    [XmlAttribute(AttributeName = "home", DataType = "anyURI")]
    public string Home;

    public void AddSlot(SlotType slotType)
    {
        Slot = [.. Slot, slotType];
    }
    public void AddSlot(string slotName, string[] valueList)
    {
        AddSlot(new()
        {
            Name = slotName,
            ValueList = new()
            {
                Value = valueList
            }
        });
    }

    public SlotType[] GetSlot(string slotName)
    {
        return Slot.Where(s => string.Equals(s.Name, slotName, StringComparison.CurrentCultureIgnoreCase)).ToArray();
    }
}
