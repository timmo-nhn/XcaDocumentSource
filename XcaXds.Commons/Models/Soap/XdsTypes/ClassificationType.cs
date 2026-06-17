using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using XcaXds.Shared.Constants;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ClassificationType : RegistryObjectType

{
    public ClassificationType()
    {
        ObjectType = Constants.Xds.ObjectTypes.Classification;
    }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "classificationScheme", DataType = "anyURI")]
    public string? ClassificationScheme { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "classifiedObject", DataType = "anyURI")]
    public string? ClassifiedObject { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "classificationNode", DataType = "anyURI")]
    public string? ClassificationNode { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "nodeRepresentation")]
    public string? NodeRepresentation { get; set; }
}
