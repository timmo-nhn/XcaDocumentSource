using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;

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

    public static XacmlPolicy TransformPolicyDtoToXacmlVersion20Policy(PolicyDto? policyDto)
    {
        if (policyDto == null) return null;

        var target = new XacmlTarget();
        var xacmlPolicy = new XacmlPolicy(new Uri(Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_DenyOverrides), target);

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


        SetXacmlPolicyRules(policyDto, xacmlEffect, xacmlPolicy);

        SetXacmlPolicyAction(policyDto, xacmlPolicy);

        SetXacmlPolicySubjects(policyDto, xacmlPolicy);

        SetXacmlPolicyResource(policyDto, xacmlPolicy);


        return xacmlPolicy;
    }

    private static void SetXacmlPolicyResource(PolicyDto policyDto, XacmlPolicy xacmlPolicy)
    {
        if (policyDto.Resources == null || policyDto.Resources?.Count == 0) return;
        var matches = new List<XacmlResourceMatch>();

        foreach (var subject in policyDto.Resources)
        {
            var xacmlResourceAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), subject.Value);
            var xacmlAttributeDesignator = new XacmlResourceAttributeDesignator(new Uri(subject.AttributeId ?? Constants.Xacml.Attribute.ResourceId), new Uri(Constants.Xacml.DataType.String));

            var xacmlResourceMatch = new XacmlResourceMatch(new Uri(Constants.Xacml.Functions.StringEqual), xacmlResourceAttributeValue, xacmlAttributeDesignator);

            matches.Add(xacmlResourceMatch);
        }

        var xacmlResources = new XacmlResource(matches);
        xacmlPolicy.Target.Resources.Add(xacmlResources);
    }

    private static void SetXacmlPolicySubjects(PolicyDto policyDto, XacmlPolicy xacmlPolicy)
    {
        if (policyDto.Subjects == null || policyDto.Subjects?.Count == 0) return;

        foreach (var subject in policyDto.Subjects)
        {
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), subject.Value?.ToString());
            var xacmlAttributeDesignator = new XacmlSubjectAttributeDesignator(new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String));

            var xacmlSubjectMatch = new XacmlSubjectMatch(new Uri(Constants.Xacml.Functions.StringEqual), xacmlActionAttributeValue, xacmlAttributeDesignator);

            var xacmlSubject = new XacmlSubject([xacmlSubjectMatch]);

            xacmlPolicy.Target.Subjects.Add(xacmlSubject);
        }
    }

    private static void SetXacmlPolicyAction(PolicyDto policyDto, XacmlPolicy xacmlPolicy)
    {
        if (policyDto.Actions == null || policyDto.Actions?.Count == 0) return;

        foreach (var action in policyDto.Actions)
        {
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), action.ToString());
            var xacmlAttributeDesignator = new XacmlActionAttributeDesignator(new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String));

            var xacmlActionMatch = new XacmlActionMatch(new Uri(Constants.Xacml.Functions.StringEqual), xacmlActionAttributeValue, xacmlAttributeDesignator);

            var xacmlAction = new XacmlAction([xacmlActionMatch]);

            xacmlPolicy.Target.Actions.Add(xacmlAction);
        }
    }

    private static void SetXacmlPolicyRules(PolicyDto policyDto, XacmlEffectType xacmlEffect, XacmlPolicy xacmlPolicy)
    {
        if (policyDto.Rules == null || policyDto.Rules?.Count == 0) return;

        var orClauseRules = new XacmlApply(new Uri(Constants.Xacml.Functions.Or));
        var xacmlRule = new XacmlRule(xacmlEffect);

        var andClause = new XacmlApply(new Uri(Constants.Xacml.Functions.And));


        foreach (var rules in policyDto.Rules)
        {
            andClause = new XacmlApply(new Uri(Constants.Xacml.Functions.And));

            foreach (var rule in rules)
            {
                var values = rule.Value?.Split(";");

                if (values?.Length > 1)
                {
                    var orClause = new XacmlApply(new Uri(Constants.Xacml.Functions.Or));
                    foreach (var value in values)
                    {
                        var stringEqual = new XacmlApply(new Uri(Constants.Xacml.Functions.StringEqual));
                        stringEqual.Parameters.Add(new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), value));

                        var stringOneAndOnly = new XacmlApply(new Uri(Constants.Xacml.Functions.StringOneAndOnly));
                        stringOneAndOnly.Parameters.Add(new XacmlSubjectAttributeDesignator(new Uri(rule.AttributeId), new Uri(Constants.Xacml.DataType.String))
                        {
                            MustBePresent = true
                        });
                        stringEqual.Parameters.Add(stringOneAndOnly);

                        orClause.Parameters.Add(stringEqual);
                    }
                    andClause.Parameters.Add(orClause);
                }
                else
                {
                    var value = values?.FirstOrDefault();

                    // If the value is parseable as URI, we are comparing AttributeDesignators instead of AttributeValues...probably
                    if (rule.CompareAttributes == true &&
                        Uri.TryCreate(value,
                        new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = true },
                        out var valueAsUri))
                    {
                        var stringEqual = new XacmlApply(new Uri(Constants.Xacml.Functions.StringEqual));

                        var firstStringOneAndOnly = new XacmlApply(new Uri(Constants.Xacml.Functions.StringOneAndOnly));
                        firstStringOneAndOnly.Parameters.Add(new XacmlSubjectAttributeDesignator(new Uri(rule.AttributeId), new Uri(Constants.Xacml.DataType.String)) { MustBePresent = true });
                        stringEqual.Parameters.Add(firstStringOneAndOnly);

                        var secondStringOneAndOnly = new XacmlApply(new Uri(Constants.Xacml.Functions.StringOneAndOnly));
                        secondStringOneAndOnly.Parameters.Add(new XacmlResourceAttributeDesignator(valueAsUri, new Uri(Constants.Xacml.DataType.String)) { MustBePresent = true });
                        stringEqual.Parameters.Add(secondStringOneAndOnly);

                        if (rule.CompareRule == CompareRule.NotEquals)
                        {
                            var notApply = new XacmlApply(new Uri(Constants.Xacml.Functions.Not));
                            notApply.Parameters.Add(stringEqual);
                            andClause.Parameters.Add(notApply);
                        }
                        else
                        {
                            andClause.Parameters.Add(stringEqual);
                        }
                    }
                    else
                    {
                        var stringEqual = new XacmlApply(new Uri(Constants.Xacml.Functions.StringEqual));
                        stringEqual.Parameters.Add(new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), value));

                        var stringOneAndOnly = new XacmlApply(new Uri(Constants.Xacml.Functions.StringOneAndOnly));
                        stringOneAndOnly.Parameters.Add(new XacmlSubjectAttributeDesignator(new Uri(rule.AttributeId), new Uri(Constants.Xacml.DataType.String)) { MustBePresent = true });
                        stringEqual.Parameters.Add(stringOneAndOnly);
                        andClause.Parameters.Add(stringEqual);

                    }
                }
            }

            orClauseRules.Parameters.Add(andClause);
        }

        if (policyDto.Rules?.Count == 1)
        {
            xacmlRule.Condition = new()
            {
                Property = andClause
            };
            xacmlPolicy.Rules.Add(xacmlRule);

        }
        else
        {
            xacmlRule.Condition = new()
            {
                Property = orClauseRules
            };
            xacmlPolicy.Rules.Add(xacmlRule);
        }

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
        if (policySetDto.CombiningAlgorithm == null)
        {
            return null;
        }

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
