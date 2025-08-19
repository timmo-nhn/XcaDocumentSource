using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Services;

public static class PolicyDtoTransformerService
{
    public static PolicyDto TransformXacmlVersion20PolicyToPolicyDto(XacmlPolicy xacmlPolicy)
    {
        var policyDto = new PolicyDto();

        var actions = xacmlPolicy.Target.Actions
            .SelectMany(action => action.Matches
            .Select(match => Enum.Parse<XacmlPolicyAction>(match.AttributeValue.Value)))
            .ToArray();

        policyDto.Action = actions;

        var organization = xacmlPolicy.Target.Subjects
            .SelectMany(sub => sub.Matches
            .Select(match => new PolicyMatch<string>() 
            {
                AttributeId = match.AttributeValue.Value, 
                MatchId = match.MatchId.ToString(),
                Value = match.AttributeValue.Value 
            }));

        

        policyDto.Subject = new()
        {
            Value = new()
            {
                
            }
        };

        return policyDto;
    }
}
