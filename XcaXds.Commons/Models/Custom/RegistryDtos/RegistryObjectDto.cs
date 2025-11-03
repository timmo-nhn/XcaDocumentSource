using System.Text.Json.Serialization;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DocumentEntryDto))]
[JsonDerivedType(typeof(SubmissionSetDto))]
[JsonDerivedType(typeof(AssociationDto))]
public class RegistryObjectDto
{
    public RegistryObjectDto()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; set; }
}