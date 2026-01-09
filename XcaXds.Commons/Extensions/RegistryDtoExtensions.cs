using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Extensions;

public static class RegistryDtoExtensions
{
    public static List<RegistryObjectDto> AsRegistryObjectList(this List<DocumentReferenceDto> documentReference)
    {
        var registryObjectDtos = new List<RegistryObjectDto>();

        foreach (var registryObject in documentReference)
        {
            registryObjectDtos.AddRange([registryObject.DocumentEntry, registryObject.SubmissionSet, registryObject.Association]);
        }

        return registryObjectDtos;
    }
}
