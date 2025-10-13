﻿using System.Text;
using System.Text.Json;
using System.Xml;
using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.Tests;

public class UnitTests_PolicyMappingDto
{
    [Fact]
    public async Task AuthZ_EvaluateFromService()
    {
        var repository = new FileBasedPolicyRepository(new Mock<ILogger<FileBasedPolicyRepository>>().Object);
        var policyWrapper = new PolicyRepositoryWrapper(repository);

        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        XacmlContextRequest xacmlObject = await PolicyRequestMapperSamlService.GetXacmlRequestFromSoapEnvelope(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18")), Encoding.UTF8), Commons.Commons.XacmlVersion.Version20);
        var requestXml = XacmlSerializer.SerializeXacmlToXml(xacmlObject, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);

        var evaluateResponse = policyWrapper.EvaluateRequest_V20(xacmlObject);
    }

    [Fact]
    public async Task AuthZ_MapFromDtoToPolicy()
    {
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "PolicyRepository"));

        foreach (var file in requests)
        {
            var jsonContent = File.ReadAllText(file);
            var policyDto = JsonSerializer.Deserialize<PolicyDto>(jsonContent, Constants.JsonDefaultOptions.DefaultSettings);

            var xacmlPolicy = PolicyDtoTransformerService.TransformPolicyDtoToXacmlVersion20Policy(policyDto);

            var xacmlPolicyString = XacmlSerializer.SerializeXacmlToXml(xacmlPolicy, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

            var policyDtoRecreated = PolicyDtoTransformerService.TransformXacmlVersion20PolicyToPolicyDto(xacmlPolicy);

        }
    }

    [Fact]
    public async Task MergePolicies()
    {
        var policy1 = new PolicyDto()
        {
            Actions = ["ReadDocument"],
            Effect = "Permit",
            Rules = 
            [
                [new("urn:oasis:names:tc:xspa:1.0:subject:role:code", "LE")]
            ]
        };

        var policy2 = new PolicyDto()
        {
            Actions = ["ReadDocument"],
            Effect = "Permit",
            Rules = 
            [
                [new("urn:oasis:names:tc:xspa:1.0:subject:role:code", "SP")]
            ]
        };


        policy1.MergeWith(policy2, true);
    }


}