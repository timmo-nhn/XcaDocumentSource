using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class SlotType
{
    [XmlElement(Order = 0)]
    public ValueListType ValueList;

    [XmlAttribute(AttributeName = "name")]
    public string Name;

    [XmlAttribute(AttributeName = "slotType", DataType = "anyURI")]
    public string SlotTypeData;

}
