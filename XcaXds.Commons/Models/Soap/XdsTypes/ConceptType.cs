using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Svs)]
public class ConceptType
{
    [XmlAttribute(AttributeName = "code")]
    public string code = string.Empty;

    [XmlAttribute(AttributeName = "codeSystemName")]
    public string codeSystemName = string.Empty;

    [XmlAttribute(AttributeName = "displayName")]
    public string displayName = string.Empty;
}