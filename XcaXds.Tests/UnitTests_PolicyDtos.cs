using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;

namespace XcaXds.Tests;

public class UnitTests_PolicyDto
{
    [Fact]
    public async Task MergePolicies()
    {
        var policy1 = new AbacPolicy()
        {
            Id="balle1",
            Actions = ["ReadDocument"],
            Effect = "Permit",
            Rules =
            [
                new((AbacCondition)new("urn:oasis:names:tc:xspa:1.0:subject:role:code", "LE"))
            ]
        };

        var policy2 = new AbacPolicy()
        {
            Id="balle2",
            Actions = ["ReadDocument"],
            Effect = "Permit",
            Rules =
            [
                new((AbacCondition)new("urn:oasis:names:tc:xspa:1.0:subject:role:code", "SP"))
            ]
        };
        
        // // policy1.MergeWith(policy2, true);
        // Assert.Equal(2, policy1.Rules?.FirstOrDefault()?.FirstOrDefault()?.Value?.Split(";").Length);
        // // policy1.MergeWith(policy2, false);
        // Assert.Equal(1, policy1.Rules?.FirstOrDefault()?.FirstOrDefault()?.Value?.Split(";").Length);
    }
}