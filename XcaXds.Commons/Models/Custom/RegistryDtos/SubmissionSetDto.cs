using System.ComponentModel.DataAnnotations;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

// HAYO! Folders are actually not supported (No field in DTO for Registrypackage.Classification)...
public class SubmissionSetDto : RegistryObjectDto
{
    [MaxLength(Constants.Properties.MaxArrayLength)]
    public List<AuthorInfo>? Author { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? AvailabilityStatus { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? HomeCommunityId { get; set; }

    public DateTime? SubmissionTime { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Title { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? UniqueId { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? SourceId { get; set; }
}