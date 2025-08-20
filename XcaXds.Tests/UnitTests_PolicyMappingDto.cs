using System.Text;
using System.Xml;
using Abc.Xacml.Context;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.Tests;

public class UnitTests_PolicyMappingDto
{
    [Fact]
    public async Task AuthZ_EvaluateFromService()
    {
        var repository = new FileBasedPolicyRepository();
        var policyWrapper = new PolicyRepositoryWrapper(repository);

        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        XacmlContextRequest xacmlObject = await PolicyRequestMapperService.GetXacml20RequestFromSoapEnvelope(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18")), Encoding.UTF8));
        var requestXml = XacmlSerializer.SerializeRequestToXml(xacmlObject);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);


        var evaluateResponse = policyWrapper.EvaluateRequest_V20(xacmlObject);
    }

    [Fact]
    public async Task AuthZ_MapFromDtoToPolicy()
    {
        var repository = new FileBasedPolicyRepository();
        var policyWrapper = new PolicyRepositoryWrapper(repository);

        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        XacmlContextRequest xacmlObject = await PolicyRequestMapperService.GetXacml20RequestFromSoapEnvelope(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18")), Encoding.UTF8));
        var requestXml = XacmlSerializer.SerializeRequestToXml(xacmlObject);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);


        var evaluateResponse = policyWrapper.EvaluateRequest_V20(xacmlObject);
    }
}