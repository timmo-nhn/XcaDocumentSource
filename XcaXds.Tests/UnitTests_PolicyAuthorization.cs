using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.Tests;

public class UnitTests_PolicyAuthorization
{
    [Fact]
    public void AuthZ_Xacml30_Policy_ShouldPermit()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))));

        XacmlContextRequest xacmlObject = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, Commons.Commons.XacmlVersion.Version20, Issuer.HelseId, new FileBasedRegistry().ReadRegistry());
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
    public void AuthZ_Xacml20_Policy_ShouldPermit()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))));

        XacmlContextRequest xacmlObject = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, Commons.Commons.XacmlVersion.Version20, Issuer.HelseId, new FileBasedRegistry().ReadRegistry());
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
    public void AuthZ_Xacml20_PolicyService()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var mockLogger = new Mock<ILogger<FileBasedPolicyRepository>>();

        var policyWrapper = new PolicyRepositoryWrapper(new FileBasedPolicyRepository(mockLogger.Object));

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))));

        XacmlContextRequest xacmlObject = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, Commons.Commons.XacmlVersion.Version20, Issuer.HelseId, new FileBasedRegistry().ReadRegistry());
        var requestXml = XacmlSerializer.SerializeXacmlToXml(xacmlObject, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);

        var evalResult = policyWrapper.EvaluateRequest_V20(xacmlObject, Issuer.HelseId);

        Assert.Equal(XacmlContextDecision.Permit, evalResult.Results.FirstOrDefault()?.Decision);
    }

    [Fact]
    public void Authz_Xacml20_JwtToXacml()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var mockLogger = new Mock<ILogger<FileBasedPolicyRepository>>();

        var jwtToken = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("JsonWebToken01.json")));
        var fhirProvideDocumentBundle = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("fhir_provideDocumentBundle.json")));

        var fhirParser = new FhirJsonParser();
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwtToken))
        {
            Assert.Fail("Invalid JWT token format.");
        }

        var token = handler.ReadJwtToken(jwtToken);

        var fhirBundle = fhirParser.Parse<Resource>(fhirProvideDocumentBundle);

        var xacmlRequest = PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(token, fhirBundle);
    }
}