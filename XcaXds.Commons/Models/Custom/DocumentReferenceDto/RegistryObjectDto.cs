using System.Text.Json.Serialization;

namespace XcaXds.Commons.Models.Custom.DocumentEntryDto;

[JsonDerivedType(typeof(DocumentEntryDto))]
[JsonDerivedType(typeof(SubmissionSetDto))]
[JsonDerivedType(typeof(AssociationDto))]
public class RegistryObjectDto
{
}