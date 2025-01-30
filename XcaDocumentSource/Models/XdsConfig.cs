public class XdsConfig
{
    public string HomeCommunityId { get; set; }
    public string RepositoryUniqueId => $"{HomeCommunityId}.2";

}
