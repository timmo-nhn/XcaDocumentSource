using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryResponseType
{
    [XmlArray(Order = 0)]
    [XmlArrayItem("Slot", Namespace = Constants.Xds.Namespaces.Rim, IsNullable = false)]
    public SlotType[] ResponseSlotList;

    [XmlElement(Order = 1)]
    public RegistryErrorList RegistryErrorList;

    [XmlAttribute(AttributeName = "status", DataType = "anyURI")]
    public string Status;

    [XmlAttribute(AttributeName = "requestId", DataType = "anyURI")]
    public string RequestId;
}
