using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[XmlInclude(typeof(RegistryObjectType))]
[XmlInclude(typeof(AdhocQueryType))]
[XmlInclude(typeof(RegistryPackageType))]
[XmlInclude(typeof(ExtrinsicObjectType))]
[XmlInclude(typeof(AssociationType))]
[XmlInclude(typeof(ExternalIdentifierType))]
[XmlInclude(typeof(ClassificationType))]
[XmlInclude(typeof(ObjectRefType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class IdentifiableType
{
    public IdentifiableType()
    {
        Id ??= Guid.NewGuid().ToString();
    }

    [XmlElement("Slot", Order = 0)]
    public SlotType[]? Slot;


    [XmlAttribute(AttributeName = "id", DataType = "anyURI")]
    public string? Id;


    [XmlAttribute(AttributeName = "home", DataType = "anyURI")]
    public string? Home;

    public void AddSlot(SlotType slotType)
    {
        Slot ??= [];
        Slot = [.. Slot, slotType];
    }

    public void AddSlot(string slotName, string?[]? valueList)
    {
        if (valueList?.OfType<string>().Any() ?? false)
        {
            Slot ??= [];
            AddSlot(new()
            {
                Name = slotName,
                ValueList = new()
                {
                    Value = valueList!
                }
            });
        }
    }

    public void UpdateSlot(string slotName, string[] valueList)
    {
        var slot = GetFirstSlot(slotName);
        if (slot == null)
        {
            AddSlot(slotName, valueList);
        }
        else
        {
            var updatedValues = slot.ValueList?.Value?.ToList() ?? new List<string>();
            updatedValues.AddRange(valueList);
            slot.ValueList ??= new();
            slot.ValueList.Value = updatedValues.Distinct().ToArray();
        }
    }

    public SlotType[] GetSlots(string slotName)
    {
        if (Slot == null) return [new SlotType()];
        try
        {
            return Slot.Where(s => string.Equals(s.Name, slotName, StringComparison.Ordinal)).ToArray();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public SlotType? GetFirstSlot(string slotName)
    {
        if (Slot == null) return new SlotType();
        return Slot.FirstOrDefault(s => string.Equals(s.Name, slotName, StringComparison.Ordinal));
    }

    public SlotType? GetFirstSlot()
    {
        if (Slot?.Length == 0) return new SlotType();
        return Slot?.FirstOrDefault();
    }
}
