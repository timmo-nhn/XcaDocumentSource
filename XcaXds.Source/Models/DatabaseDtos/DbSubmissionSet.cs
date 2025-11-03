using System.ComponentModel.DataAnnotations.Schema;
using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Models.DatabaseDtos;

public class DbSubmissionSet : DbRegistryObject
{
    public string? AvailabilityStatus { get; set; }
    public string? HomeCommunityId { get; set; }
    public string? Title { get; set; }
    public DateTime? SubmissionTime { get; set; }
    public string? SourceId { get; set; }
    public string? UniqueId { get; set; }
    public List<DbAuthorInfo> Author { get; set; } = [];
}