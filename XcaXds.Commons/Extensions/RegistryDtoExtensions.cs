using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Extensions;

public static class RegistryDtoExtensions
{
    public static List<RegistryObjectDto> AsRegistryObjectList(this List<DocumentReferenceDto> documentReference)
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
