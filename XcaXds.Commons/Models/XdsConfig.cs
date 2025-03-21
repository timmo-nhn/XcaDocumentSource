
public class XdsConfig
{
    public bool MultipartResponseForIti43 { get; set; }
    public string HomeCommunityId { get; set; }
    public string RepositoryUniqueId => $"{HomeCommunityId}.2";
    public string BackendUrl { get; set; }


}
