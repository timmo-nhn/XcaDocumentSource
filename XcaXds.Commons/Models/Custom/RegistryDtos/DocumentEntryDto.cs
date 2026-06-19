
using System.ComponentModel.DataAnnotations;
using XcaXds.Shared;
using XcaXds.Shared.Models.Custom;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class DocumentEntryDto : RegistryObjectDto
{
    [MaxLength(Constants.Properties.MaxArrayLength)]
    public List<AuthorInfo>? Author { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? AvailabilityStatus { get; set; }

    public CodedValue? ClassCode { get; set; }

    [MaxLength(Constants.Properties.MaxArrayLength)]
    public List<CodedValue>? ConfidentialityCode { get; set; }

    public DateTime? CreationTime { get; set; }

    public CodedValue? EventCodeList { get; set; }

    public CodedValue? FormatCode { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Hash { get; set; }

    public CodedValue? HealthCareFacilityTypeCode { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? HomeCommunityId { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? LanguageCode { get; set; }

    public LegalAuthenticator? LegalAuthenticator { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? MimeType { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? ObjectType { get; set; }

    public CodedValue? PracticeSettingCode { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? RepositoryUniqueId { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Size { get; set; }

    public DateTime? ServiceStartTime { get; set; }

    public DateTime? ServiceStopTime { get; set; }

    public SourcePatientInfo? SourcePatientInfo { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Title { get; set; }

    public CodedValue? TypeCode { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? UniqueId { get; set; }
}