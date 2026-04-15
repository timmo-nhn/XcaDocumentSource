using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Source;
using XcaXds.WebService.Services;

namespace XcaXds.Tests;

public class UnitTests_PolicyAuthorization
{
    [Fact]
    public void AuthZ_Xacml20_PolicyService()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData", "SoapRequests"));

        var mockLogger = new Mock<ILogger<FileBasedPolicyRepository>>();

        var policyWrapper = new PolicyRepositoryWrapper(new FileBasedPolicyRepository(mockLogger.Object), new FakeLogger<PolicyRepositoryWrapper>());

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18"))!));

        var policyrequestmappermock = new Mock<PolicyRequestMapperSamlService>();

        XacmlContextRequest xacmlRequest = policyrequestmappermock.Object.GetXacmlRequest(soapEnvelope)!;
        var requestXml = XacmlSerializer.SerializeXacmlToXml(xacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml!);

        var evalResult = policyWrapper.EvaluateRequest_V20(xacmlRequest);

        Assert.Equal(XacmlContextDecision.Permit, evalResult.Results.FirstOrDefault()?.Decision);
    }
}