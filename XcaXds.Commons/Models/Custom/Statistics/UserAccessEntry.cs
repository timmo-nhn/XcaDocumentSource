using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.Statistics;

/// <summary>
/// Used for logging user access to the system, such as
/// which endpoint they accessed, when, and what action they performed.<para/>
/// This is primarily intended for statistics and monitoring purposes.
/// </summary>
public class UserAccessEntry
{
    public string? SubjectIdHash { get; set; }
    public string? ResourceIdHash { get; set; }

    public CodedValue? SubjectOrganization { get; set; }
    public string? SubjectOrganizationName { get; set; }

    public CodedValue? SubjectChildOrganization { get; set; }
    public string? SubjectChildOrganizationName { get; set; }

    public DateTime AccessTime { get; set; }
    public string? Endpoint { get; set; }
    public string? Action { get; set; }
    public string? AccessBasis { get; set; }
    public long ElapsedTimeMillis { get; set; }
    public int ResponseStatusCode { get; set; }
    public string? SessionId { get; set; }
    public string? Issuer { get; set; }
    public CodedValue[]? DocumentConfidentialityCodes { get; set; }
    public string? SourceHostName { get; set; }
    public string? SourceHomeCommunityId { get; set; }
    public string? SourceRepositoryUniqueId { get; set; }
    public string[]? Issues { get; set; }
}
