using System.Reflection.Metadata.Ecma335;
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
            .ToList();

        policyDto.Action.AddRange(actions);

        var subjects = xacmlPolicy.Target.Subjects
            .SelectMany(sub => sub.Matches
            .Select(subjects => new PolicyMatch()
            {
                AttributeId = subjects?.AttributeSelector?.ContextSelectorId?.AbsoluteUri,
                MatchId = subjects?.MatchId?.AbsoluteUri,
                Value = subjects?.AttributeValue?.Value,

            }));

        policyDto.Subjects.AddRange(subjects);

        var resources = xacmlPolicy.Target.Resources
            .SelectMany(sub => sub.Matches
            .Select(subjects => new PolicyMatch()
            {
                AttributeId = subjects?.AttributeSelector?.ContextSelectorId?.AbsoluteUri,
                MatchId = subjects?.MatchId?.AbsoluteUri,
                Value = subjects?.AttributeValue?.Value,

            }));

        policyDto.Resources.AddRange(resources);

        return policyDto;
    }

    public static XacmlPolicy TransformPolicyDtoToXacmlVersion20Policy(PolicyDto policyDto)
    {
        var target = new XacmlTarget();
        var xacmlPolicy = new XacmlPolicy(new Uri(Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_PermitOverrides), target);



        return xacmlPolicy;
    }
}
