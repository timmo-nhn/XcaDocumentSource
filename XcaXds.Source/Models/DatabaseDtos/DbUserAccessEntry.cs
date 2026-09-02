using System.ComponentModel.DataAnnotations;
using XcaXds.Shared.Enums;

namespace XcaXds.Source.Models.DatabaseDtos;

public class DbUserAccessEntry
{
    [Key]
    public long Id { get; set; }

    [StringLength(255)]
    public string? SubjectIdHash { get; set; }

    [StringLength(255)]
    public string? ResourceIdHash { get; set; }

    public SuccessType Success { get; set; }

    [StringLength(255)]
    public string? SubjectOrganizationCode { get; set; }
    [StringLength(255)]
    public string? SubjectOrganizationCodeSystem { get; set; }
    [StringLength(255)]
    public string? SubjectOrganizationDisplayName { get; set; }

    [StringLength(255)]
    public string? SubjectOrganizationName { get; set; }

    [StringLength(255)]
    public string? SubjectChildOrganizationCode { get; set; }
    [StringLength(255)]
    public string? SubjectChildOrganizationCodeSystem { get; set; }
    [StringLength(255)]
    public string? SubjectChildOrganizationDisplayName { get; set; }

    [StringLength(255)]
    public string? SubjectChildOrganizationName { get; set; }

    public DateTime AccessTime { get; set; }

    [StringLength(1024)]
    public string? Endpoint { get; set; }

    [StringLength(255)]
    public string? Action { get; set; }

    [StringLength(255)]
    public string? AccessBasis { get; set; }

    public long ElapsedTimeMillis { get; set; }

    public int ResponseStatusCode { get; set; }

    [StringLength(255)]
    public string? SessionId { get; set; }

    [StringLength(255)]
    public string? Issuer { get; set; }

    /// <summary>JSON-serialized array of CodedValue objects.</summary>
    public string? DocumentConfidentialityCodesJson { get; set; }

    [StringLength(255)]
    public string? SourceHostName { get; set; }

    [StringLength(255)]
    public string? SourceHomeCommunityId { get; set; }

    [StringLength(255)]
    public string? SourceRepositoryUniqueId { get; set; }

    /// <summary>JSON-serialized array of issue strings.</summary>
    public string? IssuesJson { get; set; }

    public int? UploadedEntries { get; set; }
}
