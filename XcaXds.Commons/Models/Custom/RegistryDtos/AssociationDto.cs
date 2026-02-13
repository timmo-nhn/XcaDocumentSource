using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class AssociationDto : RegistryObjectDto
{
    public string? AssociationType { get; set; } = Constants.Xds.AssociationType.HasMember;
    /// <summary>
    /// Usually the RegistryPackage/SubmissionSet<para/>
    /// For RPLC associations, this is the ID of the new documententry that will replace the old entry (TargetObject)
    /// </summary>
    public string? SourceObject { get; set; }
    public string? SubmissionSetStatus { get; set; } = "Original";
    /// <summary>
    /// Usually the ExtrinsicObject/DocumentReference<para/>
    /// For RPLC associations, this is the ID of the old documententry to be replaced
    /// </summary>
    public string? TargetObject { get; set; }
}