using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Shared.Constants;
using XcaXds.Shared.Enums;
using XcaXds.Tests.Helpers;
using XcaXds.WebService.Services.PolicyEnforcementPoint;
using Xunit.Abstractions;

namespace XcaXds.Tests.IntegrationTests;

public class IntegrationTests_AccessControl(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : IntegrationTests_DefaultFixture(factory, output), IClassFixture<WebApplicationFactory<WebService.Program>>
{
    [Fact]
    public async Task Healthcarepersonell_Role_Valid_Should_GetAccess()
    {

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
            new(Constants.Saml.Attribute.XuaAcp, TestConstants.Acp.NullValue)
        );

        abacRequest = JsonSerializer.Deserialize<AbacRequest>(
            "{\n      \"attributes\": {\n        \"urn:oasis:names:tc:xspa:1.0:subject:subject-id\": [\n          \"GR\\\\u00D8NN VITS\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:role:code\": [\n          \"LE\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem\": [\n          \"urn:oid:2.16.578.1.12.4.1.1.9060\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:role:displayName\": [\n          \"Lege\"\n        ],\n        \"urn:oasis:names:tc:xspa:2.0:subject:npi\": [\n          \"565501872\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code\": [\n          \"TREAT\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:codeSystem\": [\n          \"urn:oid:2.16.840.1.113883.1.11.20448\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:displayName\": [\n          \"Treatment\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:resource:resource-id:code\": [\n          \"17855599120\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:resource:resource-id:codeSystem\": [\n          \"2.16.578.1.12.4.1.4.1\"\n        ],\n        \"urn:no:ehelse:saml:1.0:subject:SecurityLevel\": [\n          \"4\"\n        ],\n        \"urn:no:ehelse:saml:1.0:subject:Scope\": [\n          \"journaldokumenter_helsepersonell\"\n        ],\n        \"urn:oasis:names:tc:xacml:1.0:subject:subject-id\": [\n          \"GR\\\\u00D8NN VITS\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:organization\": [\n          \"STIFTELSEN BETANIEN HOSPITAL SKIEN\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:organization-id:code\": [\n          \"981275721\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:organization-id:codeSystem\": [\n          \"urn:oid:2.16.578.1.12.4.1.4.101\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:child-organization:code\": [\n          \"873255102\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:child-organization:codeSystem\": [\n          \"urn:oid:2.16.578.1.12.4.1.4.101\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:subject:role:code\": [\n          \"LE\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:subject:role:codeSystem\": [\n          \"urn:oid:2.16.578.1.12.4.1.1.9060\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:subject:role:displayName\": [\n          \"Lege\"\n        ],\n        \"urn:ihe:iti:xca:2010:homeCommunityId\": [\n          \"2.16.578.1.12.4.1.7.1.1\"\n        ],\n        \"urn:oasis:names:tc:xspa:1.0:subject:npi\": [\n          \"565501872\"\n        ],\n        \"urn:ihe:iti:xua:2017:subject:provider-identifier:code\": [\n          \"565501872\"\n        ],\n        \"urn:ihe:iti:xua:2017:subject:provider-identifier:codeSystem\": [\n          \"2.16.578.1.12.4.1.4.4\"\n        ],\n        \"urn:oasis:names:tc:xacml:1.0:resource:resource-id:code\": [\n          \"17855599120\"\n        ],\n        \"urn:oasis:names:tc:xacml:1.0:resource:resource-id:codeSystem\": [\n          \"2.16.578.1.12.4.1.4.1\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:action:purpose:code\": [\n          \"TREAT\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:action:purpose:codeSystem\": [\n          \"urn:oid:2.16.840.1.113883.1.11.20448\"\n        ],\n        \"urn:oasis:names:tc:xacml:2.0:action:purpose:displayName\": [\n          \"Treatment\"\n        ],\n        \"urn:xcads:saml:nameid\": [\n          \"05898597468\"\n        ],\n        \"urn:ihe:iti:xua:2012:acp\": [\n          \"urn:oid:2.16.578.1.12.4.1.7.2.1.0\"\n        ],\n        \"urn:oasis:names:tc:xacml:1.0:action:action-id\": [\n          \"ReadDocumentList\"\n        ],\n        \"urn:xcads:xacml:appliesto\": [\n          \"HelseId\"\n        ]\n      }\n    }"
            , Constants.JsonDefaultOptions.DefaultSettings);

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.Permit, response.Decision);
    }

    [Fact]
    public async Task Healthcarepersonell_Role_Invalid_Should_NotGetAccess()
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
            new(Constants.Saml.Attribute.Role + ":code", "XX"),
            new(Constants.Saml.Attribute.Role + ":codeSystem", "2.16.578.1.12.4.1.1.9060"),
            new(Constants.Saml.Attribute.EhelseSecurityLevel, "4"),
            new(Constants.Xacml.Attribute.ActionId, Constants.Xacml.Actions.ReadDocumentList),
            new(Constants.Urn.Custom.AppliesTo, nameof(AppliesTo.HelseId)),
            new(Constants.Saml.Attribute.XuaAcp, TestConstants.Acp.NullValue)
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.Deny, response.Decision);
    }

    [Fact]
    public async Task Healthcarepersonell_Role_NotApplicable_Should_NotGetAccess()
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
            new(Constants.Saml.Attribute.XuaAcp, TestConstants.Acp.NullValue)
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.NotApplicable, response.Decision);
    }

    [Fact]
    public async Task Healthcarepersonell_Compareattributes_Should_GetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            "AttributeCompare",
            "ReadDocumentList",
            [new AbacCondition("attribute01", AttributeCompareRule.Equals, "attribute02;attribute03", true)]
        );

        var abacRequest = new AbacRequest(
            (Constants.Xacml.Attribute.ActionId, Constants.Xacml.Actions.ReadDocumentList),
            (Constants.Urn.Custom.AppliesTo, nameof(AppliesTo.Helsenorge)),
            ("attribute01", "123123"),
            ("attribute02", "123123"),
            ("attribute03", "123123")
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.Permit, response.Decision);
    }

    [Fact]
    public async Task Healthcarepersonell_Compareattributes_Should_NotGetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            "AttributeCompare",
            "ReadDocumentList",
            [new AbacCondition("attribute01", AttributeCompareRule.Equals, "attribute02;attribute03", true)]
        );

        var abacRequest = new AbacRequest(
            new(Constants.Xacml.Attribute.ActionId, Constants.Xacml.Actions.ReadDocumentList),
            new(Constants.Urn.Custom.AppliesTo, nameof(AppliesTo.Helsenorge)),
            new("attribute01", "123123"),
            new("attribute02", "456456"),
            new("attribute03", "789789")
        );

        var response = _policyDecisionPointService.Evaluate(abacRequest);

        // Cleanup
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Decision.NotApplicable, response.Decision);
    }
}