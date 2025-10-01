using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class AssociationDto : RegistryObjectDto
{
    public string? AssociationType { get; set; } = Constants.Xds.AssociationType.HasMember;
    /// <summary>
    /// Usually the RegistryPackage/SubmissionSet
    /// </summary>
    public string? SourceObject { get; set; }
    public string? SubmissionSetStatus { get; set; } = "Original";
    /// <summary>
    /// Usually the ExtrinsicObject/DocumentReference
    /// </summary>
    public string? TargetObject { get; set; }
}