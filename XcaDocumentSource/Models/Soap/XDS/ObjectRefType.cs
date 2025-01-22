using System.ComponentModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ObjectRefType : IdentifiableType
{

    public ObjectRefType()
    {
        CreateReplica = false;
    }

    [XmlAttribute(AttributeName = "createReplica")]
    [DefaultValue(false)]
    public bool CreateReplica;
}
