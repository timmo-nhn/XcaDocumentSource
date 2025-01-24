using System.ComponentModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;


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
