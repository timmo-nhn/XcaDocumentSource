using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

public static class RegistryObjectTypeExtensions
{
    //  CREATE/UPDATE Classification
    public static void AddClassification(this RegistryObjectType registryObject, ClassificationType classificationType)
    {
        registryObject.Classification ??= [];
        registryObject.Classification = [.. registryObject.Classification, classificationType];
    }

    public static void AddClassificationRange(this RegistryObjectType registryObject, List<ClassificationType> classificationRange)
    {
        registryObject.Classification ??= [];
        registryObject.Classification = [.. registryObject.Classification, .. classificationRange];
    }

    //  CREATE/UPDATE ExternalIdentifier
    public static void AddExternalIdentifier(this RegistryObjectType registryObject, ExternalIdentifierType externalIdentifierType)
    {
        registryObject.ExternalIdentifier ??= [];
        registryObject.ExternalIdentifier = [.. registryObject.ExternalIdentifier, externalIdentifierType];
    }

    public static void AddExternalIdentifierRange(this RegistryObjectType registryObjectType, List<ExternalIdentifierType> externalIdentifierRange)
    {
        registryObjectType.ExternalIdentifier ??= [];
        registryObjectType.ExternalIdentifier = [.. registryObjectType.ExternalIdentifier, .. externalIdentifierRange];
    }
}
