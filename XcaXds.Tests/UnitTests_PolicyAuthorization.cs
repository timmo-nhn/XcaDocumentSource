using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;
using XacmlVersion = XcaXds.Commons.Commons.XacmlVersion;

namespace XcaXds.Tests;

public class UnitTests_PolicyAuthorization
{
    [Fact]
    public async Task AuthZ_Xacml30_Policy_ShouldPermit()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))));

        XacmlContextRequest xacmlObject = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, XacmlVersion.Version20, Issuer.HelseId, new FileBasedRegistry().ReadRegistry());
        var requestXml = XacmlSerializer.SerializeXacmlToXml(xacmlObject, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);

        XacmlPolicy policy01;

        var policyFile = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("Policy01_Xacml30")), Encoding.UTF8);

        using (XmlReader reader = XmlReader.Create(new StringReader(policyFile)))
        {
            var serialize = new Xacml30ProtocolSerializer();
            policy01 = serialize.ReadPolicy(reader);
        }

        var ngin = new EvaluationEngine30(policy01);

        var evalResult = ngin.Evaluate(xacmlObject, requestDoc);

        var jsoncontextresponse = JsonSerializer.Serialize(evalResult);
        Assert.Equal(XacmlContextDecision.Permit, evalResult.Results.FirstOrDefault()?.Decision);
    }

    [Fact]
    public async Task AuthZ_Xacml20_Policy_ShouldPermit()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))));

        XacmlContextRequest xacmlObject = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, XacmlVersion.Version20, Issuer.HelseId, new FileBasedRegistry().ReadRegistry());
        var requestXml = XacmlSerializer.SerializeXacmlToXml(xacmlObject, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);

        XacmlPolicy policy01;

        var policyFile = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("Policy01_Xacml20")), Encoding.UTF8);

        using (XmlReader reader = XmlReader.Create(new StringReader(policyFile)))
        {
            var serialize = new Xacml20ProtocolSerializer();
            policy01 = serialize.ReadPolicy(reader);
        }

        var ngin = new EvaluationEngine(policy01);

        var evalResult = ngin.Evaluate(xacmlObject, requestDoc);

        var jsoncontextresponse = JsonSerializer.Serialize(evalResult);
        Assert.Equal(XacmlContextDecision.Permit, evalResult.Results.FirstOrDefault()?.Decision);
    }

    [Fact]
    public async Task AuthZ_Xacml20_PolicyService()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var mockLogger = new Mock<ILogger<FileBasedPolicyRepository>>();

        var policyWrapper = new PolicyRepositoryWrapper(new FileBasedPolicyRepository(mockLogger.Object));

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))));

        XacmlContextRequest xacmlObject = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, XacmlVersion.Version20, Issuer.HelseId, new FileBasedRegistry().ReadRegistry());
        var requestXml = XacmlSerializer.SerializeXacmlToXml(xacmlObject, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);

        var evalResult = policyWrapper.EvaluateRequest_V20(xacmlObject, Issuer.HelseId);

        Assert.Equal(XacmlContextDecision.Permit, evalResult.Results.FirstOrDefault()?.Decision);
    }
}