using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyMatch<T>
{
    public string MatchId { get; set; }
    public string AttributeId { get; set; }
    public T Value { get; set; }
}