using Microsoft.AspNetCore.Mvc.Testing;
using XcaXds.Commons.Commons;
using XcaXds.Tests.Helpers;
using XcaXds.WebService.Services.PolicyEnforcementPoint;
using Xunit.Abstractions;

namespace XcaXds.Tests;

public class IntegrationTests_AccessControl : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_AccessControl(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
    public async Task AC_Healthcarepersonell_Role_Valid_Should_GetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "Healthcarepersonell_ROLE",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        var abacRequest = new AbacRequest(
            new(Constants.Saml.Attribute.Role + ":code", "SP"),
            new(Constants.Saml.Attribute.Role + ":codeSystem", "2.16.578.1.12.4.1.1.9060"),
            new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),
            new(Constants.Xacml.Attribute.ActionId, Constants.Xacml.Actions.ReadDocumentList),
            new(Constants.Urn.Custom.AppliesTo, nameof(AppliesTo.HelseId)),
            new(Constants.Saml.Attribute.XuaAcp, Constants.Oid.Saml.Acp.NullValue)
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.Permit, response.Decision);
    }

    [Fact]
    public async Task AC_Healthcarepersonell_Role_Invalid_Should_NotGetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddRandomAccessControlPolicies(_policyRepositoryService, 100);
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "Healthcarepersonell_ROLE",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        var abacRequest = new AbacRequest(
            new(Constants.Saml.Attribute.Role + ":code", "XX"),
            new(Constants.Saml.Attribute.Role + ":codeSystem", "2.16.578.1.12.4.1.1.9060"),
            new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),
            new(Constants.Xacml.Attribute.ActionId, Constants.Xacml.Actions.ReadDocumentList),
            new(Constants.Urn.Custom.AppliesTo, nameof(AppliesTo.HelseId)),
            new(Constants.Saml.Attribute.XuaAcp, Constants.Oid.Saml.Acp.NullValue)
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.Deny, response.Decision);
    }

    [Fact]
    public async Task AC_Healthcarepersonell_Role_NotApplicable_Should_NotGetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "Healthcarepersonell_ROLE",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        var abacRequest = new AbacRequest(
            new(Constants.Xacml.Attribute.ActionId, Constants.Xacml.Actions.Create),
            new(Constants.Urn.Custom.AppliesTo, nameof(AppliesTo.HelseId)),
            new(Constants.Saml.Attribute.XuaAcp, Constants.Oid.Saml.Acp.NullValue)
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.NotApplicable, response.Decision);
    }
}