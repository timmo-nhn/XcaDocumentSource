using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Commons.Extensions;

public static class PolicyDtoExtensions
{
    // public static AbacPolicy WithId(this AbacPolicy abacPolicy, string identifier)
    // {
    //     abacPolicy.Id = identifier;
    //
    //     return abacPolicy;
    // }
    //
    // public static AbacPolicy AppliesTo(this AbacPolicy abacPolicy, AppliesTo issuer)
    // {
    //     abacPolicy.AppliesTo ??= new();
    //     if (abacPolicy.AppliesTo.Contains(issuer) == false)
    //     {
    //         abacPolicy.AppliesTo.Add(issuer);
    //     }
    //
    //     return abacPolicy;
    // }
    //
    // public static AbacPolicy AddAction(this AbacPolicy abacPolicy, string action)
    // {
    //     abacPolicy.Actions ??= new();
    //     abacPolicy.Actions.Add(action);
    //
    //     return abacPolicy;
    // }
    //
    // public static AbacPolicy AddRule(this AbacPolicy abacPolicy, string attributeId, AttributeCompareRule compareRule, string value)
    // {
    //     abacPolicy.Rules ??= [[]];
    //     abacPolicy.Rules.Add([new(attributeId, compareRule, value)]);
    //
    //     return abacPolicy;
    // }
    //
    // public static AbacPolicy AddRule(this AbacPolicy abacPolicy, string attributeId, string value)
    // {
    //     abacPolicy.Rules ??= [[]];
    //     abacPolicy.Rules.Add([new(attributeId, value)]);
    //
    //     return abacPolicy;
    // }

    // public static List<PolicyMatch> MergeWith(this List<PolicyMatch>? matches, IEnumerable<PolicyMatch>? patch)
    // {
    //     var result = matches ?? new();
    //
    //     foreach (var patchRule in patch ?? Enumerable.Empty<PolicyMatch>())
    //     {
    //         var idx = result.FindIndex(r => r.AttributeId == patchRule.AttributeId);
    //
    //         if (idx < 0)
    //         {
    //             result.Add(patchRule);
    //             continue;
    //         }
    //
    //         result[idx] = new PolicyMatch
    //         {
    //             AttributeId = patchRule.AttributeId,
    //             Value = patchRule.Value
    //         };
    //     }
    //
    //     return result;
    // }
}