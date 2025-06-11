using System.Text.Json.Serialization;

namespace XcaXds.Commons.Models.Custom.DocumentEntry;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DocumentEntryDto))]
[JsonDerivedType(typeof(SubmissionSetDto))]
[JsonDerivedType(typeof(AssociationDto))]
public class RegistryObjectDto
{
    public string Id { get; set; }
}