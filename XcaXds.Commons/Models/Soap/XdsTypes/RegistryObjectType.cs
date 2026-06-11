using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[XmlInclude(typeof(AdhocQueryType))]
[XmlInclude(typeof(RegistryPackageType))]
[XmlInclude(typeof(ExtrinsicObjectType))]
[XmlInclude(typeof(AssociationType))]
[XmlInclude(typeof(ExternalIdentifierType))]
[XmlInclude(typeof(ClassificationType))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class RegistryObjectType : IdentifiableType
{
    [XmlElement]
    public InternationalStringType? Name { get; set; }

    [XmlElement]
    public InternationalStringType? Description { get; set; }

    [MaxLength(Constants.Properties.MaxArrayLength)]
    [XmlElement("Classification")]
    public ClassificationType[]? Classification { get; set; }

    [MaxLength(Constants.Properties.MaxArrayLength)]
    [XmlElement("ExternalIdentifier")]
    public ExternalIdentifierType[]? ExternalIdentifier { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "lid", DataType = "anyURI")]
    public string? Lid { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "objectType", DataType = "anyURI")]
    public string? ObjectType { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    [XmlAttribute(AttributeName = "status", DataType = "anyURI")]
    public string? Status { get; set; }

    public ClassificationType[] GetClassifications(string classificationScheme)
    {
        return Classification?.Where(cl => cl?.ClassificationScheme == classificationScheme).ToArray() ?? [];
    }

    public ClassificationType? GetFirstClassification(string classificationScheme)
    {
        return Classification?.FirstOrDefault(cl => cl?.ClassificationScheme == classificationScheme);
    }

    public ExternalIdentifierType[] GetExternalIdentifiers(string identificationScheme)
    {
        return ExternalIdentifier?.Where(cl => cl?.IdentificationScheme == identificationScheme).ToArray() ?? [];
    }

    public ExternalIdentifierType? GetFirstExternalIdentifier(string identificationScheme)
    {
        return ExternalIdentifier?.FirstOrDefault(cl => cl?.IdentificationScheme == identificationScheme);
    }

}
