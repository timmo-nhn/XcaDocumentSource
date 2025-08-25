using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Services;

/// <summary>
/// Transform between simple Policy DTO objects and actual XACML objects. <para/>
/// Policy DTOs are used for storage and creation, whilst the actual policy evaluation is done via XACML XML-requests and policies
/// </summary>
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

        policyDto.Effect = xacmlPolicy.Rules.FirstOrDefault()?.Effect.ToString() ?? XacmlEffectType.Permit.ToString();

        return policyDto;
    }

    public static XacmlPolicy TransformPolicyDtoToXacmlVersion20Policy(PolicyDto policyDto)
    {
        var target = new XacmlTarget();
        var xacmlPolicy = new XacmlPolicy(new Uri(Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_PermitOverrides), target);

        XacmlEffectType xacmlEffect;

        if (Enum.TryParse<XacmlEffectType>(policyDto.Effect, ignoreCase: true, out var effect))
        {
            xacmlEffect = effect;
        }
        else
        {
            throw new ArgumentException("policy effect");
        }


        if (policyDto.Id != null)
        {
            xacmlPolicy.PolicyId = new Uri($"urn:uuid:{policyDto.Id}", UriKind.Absolute);
        }


        foreach (var action in policyDto.Actions ?? new List<string>())
        {
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), action.ToString());
            var xacmlAttributeDesignator = new XacmlActionAttributeDesignator(new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String));

            var xacmlActionMatch = new XacmlActionMatch(new Uri(Constants.Xacml.Functions.StringEqual), xacmlActionAttributeValue, xacmlAttributeDesignator);

            var xacmlAction = new XacmlAction([xacmlActionMatch]);
            xacmlPolicy.Target.Actions.Add(xacmlAction);
        }

        foreach (var subject in policyDto.Subjects ?? new List<PolicyMatch>())
        {
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), subject.Value?.ToString());
            var xacmlAttributeDesignator = new XacmlSubjectAttributeDesignator(new Uri(Constants.Xacml.Attribute.SubjectId), new Uri(Constants.Xacml.DataType.String));

            var xacmlActionMatch = new XacmlSubjectMatch(new Uri(Constants.Xacml.Functions.StringEqual), xacmlActionAttributeValue, xacmlAttributeDesignator);

            var xacmlAction = new XacmlSubject([xacmlActionMatch]);
            xacmlPolicy.Target.Subjects.Add(xacmlAction);
        }

        foreach (var resource in policyDto.Resources ?? new List<PolicyMatch>())
        {
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), resource.Value?.ToString());
            var xacmlAttributeDesignator = new XacmlResourceAttributeDesignator(new Uri(Constants.Xacml.Attribute.ResourceId), new Uri(Constants.Xacml.DataType.String));

            var xacmlActionMatch = new XacmlResourceMatch(new Uri(Constants.Xacml.Functions.StringEqual), xacmlActionAttributeValue, xacmlAttributeDesignator);

            var xacmlAction = new XacmlResource([xacmlActionMatch]);
            xacmlPolicy.Target.Resources.Add(xacmlAction);
        }


        if (policyDto.Rules != null && policyDto.Rules.Count != 0)
        {
            var andApply = new XacmlApply(new Uri(Constants.Xacml.Functions.And));

            foreach (var rule in policyDto.Rules)
            {
                var values = rule.Value?.Split(";");

                if (values?.Length > 1)
                {
                    var orClause = new XacmlApply(new Uri(Constants.Xacml.Functions.Or));
                    foreach (var value in values)
                    {
                        var stringApply = new XacmlApply(new Uri(Constants.Xacml.Functions.StringEqual));
                        stringApply.Parameters.Add(new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), value));
                        stringApply.Parameters.Add(new XacmlSubjectAttributeDesignator(new Uri(rule.AttributeId), new Uri(Constants.Xacml.DataType.String)));
                        orClause.Parameters.Add(stringApply);
                    }

                    andApply.Parameters.Add(orClause);
                }
                else
                {
                    var apply = new XacmlApply(new Uri(Constants.Xacml.Functions.StringEqual));
                    apply.Parameters.Add(new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), values.FirstOrDefault()));
                    apply.Parameters.Add(new XacmlSubjectAttributeDesignator(new Uri(rule.AttributeId), new Uri(Constants.Xacml.DataType.String)));

                    andApply.Parameters.Add(apply);
                }

            }

        }

        var xacmlRule = new XacmlRule(xacmlEffect);

        xacmlRule.Condition = new XacmlExpression()
        {
            Property = andApply
        };

        xacmlPolicy.Rules.Add(xacmlRule);

        if (policyDto.Resources != null && policyDto.Resources.Count != 0)
        {
            var matches = new List<XacmlResourceMatch>();

            foreach (var subject in policyDto.Resources)
            {
                var xacmlResourceAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), subject.Value);
                var xacmlAttributeDesignator = new XacmlResourceAttributeDesignator(new Uri(subject.AttributeId ?? Constants.Xacml.Attribute.ResourceId), new Uri(Constants.Xacml.DataType.String));

                var xacmlResourceMatch = new XacmlResourceMatch(new Uri(subject.MatchId ?? Constants.Xacml.Functions.StringEqual), xacmlResourceAttributeValue, xacmlAttributeDesignator);

                matches.Add(xacmlResourceMatch);

            }

            var xacmlResources = new XacmlResource(matches);
            xacmlPolicy.Target.Resources.Add(xacmlResources);
        }

        var debug_policy = XacmlSerializer.SerializeXacmlToXml(xacmlPolicy, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        return xacmlPolicy;
    }

    public static PolicySetDto TransformXacmlVersion20PolicySetToPolicySetDto(XacmlPolicySet xacmlPolicySet)
    {
        var policySetDto = new PolicySetDto();

        if (xacmlPolicySet.Policies.Count != 0)
        {
            foreach (var xacmlPolicy in xacmlPolicySet.Policies)
            {
                policySetDto.Policies ??= new();
                policySetDto.Policies.Add(TransformXacmlVersion20PolicyToPolicyDto(xacmlPolicy));
            }
        }

        return policySetDto;

    }

    public static XacmlPolicySet? TransformPolicySetDtoToXacmlVersion20PolicySet(PolicySetDto policySetDto)
    {
        var xacmlPolicySet = new XacmlPolicySet(new Uri(policySetDto.CombiningAlgorithm), new XacmlTarget());

        xacmlPolicySet.PolicySetId = new Uri($"urn:uuid:{policySetDto.SetId}", UriKind.Absolute);

        if (policySetDto.Policies?.Count != 0)
        {
            foreach (var policyDto in policySetDto.Policies ?? new List<PolicyDto>())
            {
                xacmlPolicySet.Policies.Add(TransformPolicyDtoToXacmlVersion20Policy(policyDto));
            }
        }

        return xacmlPolicySet;
    }
}
