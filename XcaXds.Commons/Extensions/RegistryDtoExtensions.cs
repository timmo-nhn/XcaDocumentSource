using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

public static class RegistryDtoExtensions
{
    public static List<IdentifiableType> AsRegistryObjectList(this List<DocumentReferenceDto> documentReference)
    {
        return RegistryMetadataTransformer.TransformRegistryObjectDtosToRegistryObjects(documentReference.AsRegistryObjectDtoList()).ToList();
    }

    public static List<RegistryObjectDto> AsRegistryObjectDtoList(this List<DocumentReferenceDto> documentReference)
    {
        var registryObjectDtos = new List<RegistryObjectDto>();

        foreach (var registryObject in documentReference)
        {
            if (registryObject.DocumentEntry != null)
            {
                registryObjectDtos.Add(registryObject.DocumentEntry);
            }
            if (registryObject.SubmissionSet != null)
            {
                registryObjectDtos.Add(registryObject.SubmissionSet);
            }
            if (registryObject.Association != null)
            {
                registryObjectDtos.Add(registryObject.Association);
            }
        }

        return registryObjectDtos;
    }
}
