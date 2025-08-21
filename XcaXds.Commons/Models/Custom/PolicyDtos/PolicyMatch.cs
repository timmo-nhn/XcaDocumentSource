using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyMatch
{
    public string? MatchId { get; set; }
    public string? AttributeId { get; set; }
    public string? DataType { get; set; }
    public string? Value { get; set; }
}