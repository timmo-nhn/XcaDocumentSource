using System.ComponentModel.DataAnnotations.Schema;
using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Models.DatabaseDtos;

public class DbDocumentEntry : DbRegistryObject
{
    public string? MimeType { get; set; }
    public string? Hash { get; set; }
    public string? RepositoryUniqueId { get; set; }
    public string? Size { get; set; }
    public string? ObjectType { get; set; }
    public string? AvailabilityStatus { get; set; }
    public string? HomeCommunityId { get; set; }
    public string? UniqueId { get; set; }
    public string? Title { get; set; }
    public string? LanguageCode { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? ServiceStartTime { get; set; }
    public DateTime? ServiceStopTime { get; set; }
    public DbCodedValue? ClassCode { get; set; }
    public DbCodedValue? TypeCode { get; set; }
    public DbCodedValue? FormatCode { get; set; }
    public DbCodedValue? EventCodeList { get; set; }
    public DbCodedValue? PracticeSettingCode { get; set; }
    public DbCodedValue? HealthCareFacilityTypeCode { get; set; }
    public DbLegalAuthenticator? LegalAuthenticator { get; set; }
    public DbSourcePatientInfo? SourcePatientInfo { get; set; }
    public List<DbAuthorInfo> Author { get; set; } = [];
    public List<DbCodedValue> ConfidentialityCode { get; set; } = [];
}