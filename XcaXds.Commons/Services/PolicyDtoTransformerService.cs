using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Commons.Services;

public static class PolicyDtoTransformerService
{
    public static PolicyDto TransformXacmlVersion20PolicyToPolicyDto(XacmlPolicy xacmlPolicy)
    {
        var policyDto = new PolicyDto();

        policyDto.Id = xacmlPolicy.PolicyId.ToString();

        var actions = xacmlPolicy.Target.Actions
            .SelectMany(action => action.Matches
            .Select(match => match.AttributeValue.Value))
            .ToList();

        if (actions.Count != 0)
        {
            policyDto.Actions ??= new();
            policyDto.Actions.AddRange(actions);
        }

        var subjects = xacmlPolicy.Target.Subjects
            .SelectMany(sub => sub.Matches
            .Select(subjects => new PolicyMatch()
            {
                AttributeId = subjects?.AttributeDesignator?.AttributeId?.ToString(),
                DataType = subjects?.AttributeDesignator?.DataType?.ToString(),
                MatchId = subjects?.MatchId?.ToString(),
                Value = subjects?.AttributeValue?.Value,
            })).ToList();

        if (subjects.Count != 0)
        {
            policyDto.Subjects ??= new();
            policyDto.Subjects.AddRange(subjects);
        }

        var resources = xacmlPolicy.Target.Resources
            .SelectMany(res => res.Matches
            .Select(resources => new PolicyMatch()
            {
                AttributeId = resources?.AttributeSelector?.ContextSelectorId?.ToString(),
                DataType = resources?.AttributeDesignator?.DataType?.ToString(),
                MatchId = resources?.MatchId?.ToString(),
                Value = resources?.AttributeValue?.Value,

            })).ToList();

        if (resources.Count != 0)
        {
            policyDto.Resources ??= new();
            policyDto.Resources.AddRange(resources);
        }

        policyDto.Effect = xacmlPolicy.Rules.FirstOrDefault()?.Effect ?? XacmlEffectType.Permit;

        return policyDto;
    }

    public static XacmlPolicy TransformPolicyDtoToXacmlVersion20Policy(PolicyDto policyDto)
    {
        var target = new XacmlTarget();
        var xacmlPolicy = new XacmlPolicy(new Uri(Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_PermitOverrides), target);

        if (policyDto.Id != null)
        {
            xacmlPolicy.PolicyId = new Uri(policyDto.Id);
        }

        foreach (var action in policyDto.Actions ?? new List<string>())
        {
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), action.ToString());
            var xacmlAttributeDesignator = new XacmlActionAttributeDesignator(new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String));

            var xacmlActionMatch = new XacmlActionMatch(new Uri(Constants.Xacml.Attribute.ActionId), xacmlActionAttributeValue, xacmlAttributeDesignator);

            var xacmlAction = new XacmlAction([xacmlActionMatch]);

            xacmlPolicy.Target.Actions.Add(xacmlAction);
        }

        foreach (var subject in policyDto.Subjects ?? new List<PolicyMatch>())
        {
            var xacmlSubjectAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), subject.Value);
            var xacmlAttributeDesignator = new XacmlSubjectAttributeDesignator(new Uri(subject.AttributeId), new Uri(Constants.Xacml.DataType.String));

            var xacmlSubjectMatch = new XacmlSubjectMatch(new Uri(subject.MatchId), xacmlSubjectAttributeValue, xacmlAttributeDesignator);

            var xacmlSubject = new XacmlSubject([xacmlSubjectMatch]);

            xacmlPolicy.Target.Subjects.Add(xacmlSubject);
        }


        foreach (var subject in policyDto.Resources ?? new List<PolicyMatch>())
        {
            var xacmlResourceAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), subject.Value);
            var xacmlAttributeDesignator = new XacmlResourceAttributeDesignator(new Uri(subject.AttributeId), new Uri(Constants.Xacml.DataType.String));

            var xacmlResourceMatch = new XacmlResourceMatch(new Uri(subject.MatchId), xacmlResourceAttributeValue, xacmlAttributeDesignator);

            var xacmlSubject = new XacmlResource([xacmlResourceMatch]);

            xacmlPolicy.Target.Resources.Add(xacmlSubject);
        }

        xacmlPolicy.Rules.Add(new XacmlRule(policyDto.Effect));
        
        return xacmlPolicy;
    }
}
