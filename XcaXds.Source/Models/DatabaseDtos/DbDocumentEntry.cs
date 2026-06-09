using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using XcaXds.Source.Models.DatabaseDtos.Types;

namespace XcaXds.Source.Models.DatabaseDtos;

[Index(nameof(DE_UniqueId), IsUnique = true)]
[Index(nameof(DE_HomeCommunityId), IsUnique = false)]
[Index(nameof(DE_RepositoryUniqueId), IsUnique = false)]
public class DbDocumentEntry : DbRegistryObject
{
    [StringLength(255)]
    public string? DE_MimeType { get; set; }
    [StringLength(255)]
    public string? DE_Hash { get; set; }
    [StringLength(255)]
    public string? DE_RepositoryUniqueId { get; set; }
    [StringLength(255)]
    public string? DE_Size { get; set; }
    [StringLength(255)]
    public string? DE_ObjectType { get; set; }
    [StringLength(255)]
    public string? DE_AvailabilityStatus { get; set; }
    [StringLength(255)]
    public string? DE_HomeCommunityId { get; set; }
    [StringLength(255)]
    public string? DE_UniqueId { get; set; }
    [StringLength(255)]
    public string? DE_Title { get; set; }
    [StringLength(255)]
    public string? DE_LanguageCode { get; set; }
    public DateTime? DE_CreationTime { get; set; }
    public DateTime? DE_ServiceStartTime { get; set; }
    public DateTime? DE_ServiceStopTime { get; set; }
    public DbCodedValue? DE_ClassCode { get; set; }
    public DbCodedValue? DE_TypeCode { get; set; }
    public DbCodedValue? DE_FormatCode { get; set; }
    public DbCodedValue? DE_EventCodeList { get; set; }
    public DbCodedValue? DE_PracticeSettingCode { get; set; }
    public DbCodedValue? DE_HealthCareFacilityTypeCode { get; set; }
    public DbLegalAuthenticator? DE_LegalAuthenticator { get; set; }
    [StringLength(255)]
    public required string DE_SourcePatientInfoPatientId { get; init; }
    [StringLength(255)]
    public required string DE_SourcePatientInfoPatientSystem { get; init; }
    [StringLength(255)]
    public string? DE_SourcePatientInfoFirstName { get; set; }
    [StringLength(255)]
    public string? DE_SourcePatientInfoLastName { get; set; }
    public DateTime? DE_SourcePatientInfoBirthTime { get; set; }
    public DbCodedValue? DE_SourcePatientInfoGender { get; set; }
    public List<DbAuthorInfo> DE_Author { get; set; } = [];
    public List<DbCodedValue> DE_ConfidentialityCode { get; set; } = [];
}