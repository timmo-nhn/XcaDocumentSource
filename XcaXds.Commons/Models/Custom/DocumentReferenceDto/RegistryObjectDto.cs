using System.Text.Json.Serialization;

namespace XcaXds.Commons.Models.Custom.DocumentEntryDto;

[JsonDerivedType(typeof(DocumentEntryDto))]
[JsonDerivedType(typeof(AssociationDto))]
[JsonDerivedType(typeof(SubmissionSetDto))]
public class RegistryObjectDto
{
}