using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyDto
{
    public string? Id { get; set; }
    public List<List<PolicyMatch>>? Rules { get; set; }
    public List<PolicyMatch>? Subjects { get; set; }
    public List<PolicyMatch>? Roles { get; set; }
    public List<PolicyMatch>? Resources { get; set; }
    public List<string>? Actions { get; set; }
    public string? Effect { get; set; }

    public void MergeWith(PolicyDto? patch, bool append)
    {
        foreach (var patchRules in patch?.Rules ?? [])
        {
            foreach (var patchRule in patchRules)
            {
                var idx = patchRules.FindIndex(rule => rule.AttributeId == patchRule.AttributeId);

                if (idx < 0)
                {
                    patchRules.Add(patchRule);
                    continue;
                }

                if (append == true)
                {
                    var existing = patchRules[idx];
                    patchRules[idx] = new PolicyMatch()
                    {
                        AttributeId = existing.AttributeId,
                        Value = string.Join(';', (existing.Value + ";" + patchRule.Value)
                        .TrimStart(';').TrimEnd(';')
                        .Split(";")
                        .Distinct())
                    };
                }
                else
                {
                    patchRules[idx] = new PolicyMatch
                    {
                        AttributeId = patchRule.AttributeId,
                        Value = patchRule.Value
                    };
                }
            }
        }

        foreach (var patchRule in patch?.Subjects ?? [])
        {
            var idx = Subjects.FindIndex(rule => rule.AttributeId == patchRule.AttributeId);

            if (idx < 0)
            {
                Subjects.Add(patchRule);
                continue;
            }

            Subjects[idx] = new PolicyMatch
            {
                AttributeId = patchRule.AttributeId,
                Value = patchRule.Value
            };
        }

        foreach (var patchRule in patch?.Resources ?? [])
        {
            var idx = Resources.FindIndex(rule => rule.AttributeId == patchRule.AttributeId);

            if (idx < 0)
            {
                Resources.Add(patchRule);
                continue;
            }

            Resources[idx] = new PolicyMatch
            {
                AttributeId = patchRule.AttributeId,
                Value = patchRule.Value
            };
        }

        foreach (var patchRule in patch?.Roles ?? [])
        {
            var idx = Roles.FindIndex(rule => rule.AttributeId == patchRule.AttributeId);

            if (idx < 0)
            {
                Roles.Add(patchRule);
                continue;
            }

            Roles[idx] = new PolicyMatch
            {
                AttributeId = patchRule.AttributeId,
                Value = patchRule.Value
            };
        }
    }

    public void SetDefaultValues()
    {
        if (Subjects != null)
        {
            foreach (var item in Subjects)
            {
                item.AttributeId ??= Constants.Xacml.Attribute.SubjectId;
            }
        }

        if (Roles != null)
        {
            foreach (var item in Roles)
            {
                item.AttributeId ??= Constants.Xacml.Attribute.Role;
            }
        }

        if (Resources != null)
        {
            foreach (var item in Resources)
            {
                item.AttributeId ??= Constants.Xacml.Attribute.ResourceId;
            }
        }
    }
}