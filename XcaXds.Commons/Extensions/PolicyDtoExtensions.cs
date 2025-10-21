using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Commons.Extensions;

public static class PolicyDtoExtensions
{
    public static PolicyDto WithId(this PolicyDto policyDto, string identifier)
    {
        policyDto.Id = identifier;

        return policyDto;
    }

    public static PolicyDto AppliesTo(this PolicyDto policyDto, Issuer issuer)
    {
        policyDto.AppliesTo ??= new();
        if (policyDto.AppliesTo.Contains(issuer) == false)
        {
            policyDto.AppliesTo.Add(issuer);
        }

        return policyDto;
    }

    public static PolicyDto AddAction(this PolicyDto policyDto, string action)
    {
        policyDto.Actions ??= new();
        policyDto.Actions.Add(action);

        return policyDto;
    }

    public static PolicyDto AddRule(this PolicyDto policyDto, string attributeId, CompareRule compareRule, string value)
    {
        policyDto.Rules ??= [[]];
        policyDto.Rules.Add([new(attributeId, compareRule, value)]);

        return policyDto;
    }

    public static PolicyDto AddRule(this PolicyDto policyDto, string attributeId, string value)
    {
        policyDto.Rules ??= [[]];
        policyDto.Rules.Add([new(attributeId, value)]);

        return policyDto;
    }
}