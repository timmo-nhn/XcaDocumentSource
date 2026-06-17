using System.Xml.Serialization;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rs)]
public class RegistryRequestType
{
    [XmlArray]
    [XmlArrayItem("Slot", Namespace = Constants.Xds.Namespaces.Rim, IsNullable = false)]
    public SlotType[]? RequestSlotList { get; set; }

    [XmlAttribute(AttributeName = "id", DataType = "anyURI")]
    public string? Id { get; set; }

    [XmlAttribute(AttributeName = "comment")]
    public string? Comment { get; set; }

}
