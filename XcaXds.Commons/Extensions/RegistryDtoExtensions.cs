using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

public static class RegistryDtoExtensions
{
    public static IEnumerable<IdentifiableType> AsRegistryObjectList(this IEnumerable<DocumentReferenceDto> documentReference)
    {
        foreach (var registryObjectDtos in documentReference.AsRegistryObjectDtos())
        {
            var registryObject = RegistryMetadataTransformer.TransformRegistryObjectDtoToRegistryObject(registryObjectDtos);
            if (registryObject == null) continue;

            yield return registryObject;
        }
    }

    public static IEnumerable<RegistryObjectDto> AsRegistryObjectDtos(this IEnumerable<DocumentReferenceDto> documentReference)
    {
        foreach (var registryObject in documentReference)
        {
            if (registryObject.DocumentEntry != null)
            {
                yield return registryObject.DocumentEntry;
            }
            if (registryObject.SubmissionSet != null)
            {
                yield return registryObject.SubmissionSet;
            }
            if (registryObject.Association != null)
            {
                yield return registryObject.Association;
            }
        }
    }
}
