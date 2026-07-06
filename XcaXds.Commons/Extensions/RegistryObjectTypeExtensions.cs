using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

public static class RegistryObjectTypeExtensions
{
    public static void AddClassification(this RegistryObjectType registryObject, ClassificationType classificationType)
    {
        registryObject.Classification ??= [];
        registryObject.Classification = [.. registryObject.Classification, classificationType];
    }

    public static void AddClassificationRange(this RegistryObjectType registryObject, IEnumerable<ClassificationType> classificationRange)
    {
        registryObject.Classification ??= [];
        registryObject.Classification = [.. registryObject.Classification, .. classificationRange];
    }

    public static void AddExternalIdentifier(this RegistryObjectType registryObject, ExternalIdentifierType externalIdentifierType)
    {
        registryObject.ExternalIdentifier ??= [];
        registryObject.ExternalIdentifier = [.. registryObject.ExternalIdentifier, externalIdentifierType];
    }

    public static void AddExternalIdentifierRange(this RegistryObjectType registryObjectType, IEnumerable<ExternalIdentifierType> externalIdentifierRange)
    {
        registryObjectType.ExternalIdentifier ??= [];
        registryObjectType.ExternalIdentifier = [.. registryObjectType.ExternalIdentifier, .. externalIdentifierRange];
    }
}
