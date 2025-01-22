using System.ComponentModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class FederationType : RegistryObjectType
{
    public FederationType()
    {
        ReplicationSyncLatency = "P1D";
    }

    [XmlAttribute(AttributeName = "replicationSyncLatency", DataType = "duration")]
    [DefaultValue("P1D")]
    public string ReplicationSyncLatency;
}
