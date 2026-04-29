using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using XcaXds.Commons.Commons;

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

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string Id { get; set; }
}