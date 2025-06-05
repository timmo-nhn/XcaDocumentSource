using System.Text.Json.Serialization;

namespace XcaXds.Commons.Models.Custom.DocumentEntryDto;

[JsonPolymorphic(TypeDiscriminatorPropertyName ="$type")]
[JsonDerivedType(typeof(DocumentEntryDto))]
[JsonDerivedType(typeof(SubmissionSetDto))]
[JsonDerivedType(typeof(AssociationDto))]
public class RegistryObjectDto
{
}