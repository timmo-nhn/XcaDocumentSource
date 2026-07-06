using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Shared.Enums;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class AbacPolicy
{
    public List<AppliesTo>? AppliesTo { get; set; }
    public required string Id { get; init; }
    public List<AbacRuleGroup>? Rules { get; set; }
    public string? Description { get; set; }
    public string Effect { get; set; } = "Deny";
    public List<string>? Actions { get; set; }
}

public class AbacRuleGroup
{
    public List<AbacCondition> Conditions { get; set; } = [];

    public AbacRuleGroup() { }

    public AbacRuleGroup(AbacCondition condition)
    {
        Conditions = [condition];
    }

    public AbacRuleGroup(params AbacCondition[] conditions)
    {
        Conditions = conditions.ToList();
    }
}