namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class DocumentStatusDto
{
    public string? DocumentId { get; set; }
	public string? HomeCommunityId { get; set; }
	public string? RepositoryUniqueId { get; set; }
	public bool IsFullySaved { get; set; } = false; // saved both in registry and repository
	public bool SavedToRegistry { get; set; } = false;
	public bool SavedToRepository { get; set; } = false;
}