using System.Text;
using System.Text.Json;
using System.Xml;
using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
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

        XacmlContextRequest xacmlObject = await PolicyRequestMapperSamlService.GetXacml20RequestFromSoapEnvelope(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18")), Encoding.UTF8));
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
            var policyXmlString = File.ReadAllText(file);
            
            XacmlPolicy policy;

            using (XmlReader reader = XmlReader.Create(new StringReader(policyXmlString)))
            {
                var serialize = new Xacml20ProtocolSerializer();
                policy = serialize.ReadPolicy(reader);
            }

            var policyDto = PolicyDtoTransformerService.TransformXacmlVersion20PolicyToPolicyDto(policy);



            var policyJson = JsonSerializer.Serialize(policyDto, Constants.JsonDefaultOptions.DefaultSettings);

            var xacmlPolicyReCreated = PolicyDtoTransformerService.TransformPolicyDtoToXacmlVersion20Policy(policyDto);
        }
    }
}