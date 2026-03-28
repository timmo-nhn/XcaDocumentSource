using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Models.DatabaseDtos;

public class DbSubmissionSet : DbRegistryObject
{
    public string? SS_AvailabilityStatus { get; set; }
    public string? SS_HomeCommunityId { get; set; }
    public string? SS_Title { get; set; }
    public DateTime? SS_SubmissionTime { get; set; }
    public string? SS_SourceId { get; set; }
    public string? SS_UniqueId { get; set; }
    public List<DbAuthorInfo> SS_Author { get; set; } = [];
}