using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Runtime;
using System.Text;
using System.Xml;
using XcaXds.Commons.Services;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Services;

namespace XcaXds.Tests;

public class UnitTests_PolicyAuthorization
{
    [Fact]
    public async Task Authorization_PolicyStuff()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData","Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var policyService = new PolicyAuthorizationService();

        var request = new XmlDocument();
        var xacmlObject = await policyService.GetXacmlRequestFromSoapEnvelope(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18")), Encoding.UTF8));
        var requestFile = XacmlSerializer.SerializeRequestToXml(xacmlObject);

        request.LoadXml(requestFile);

        var policyFile = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("Policy01")),Encoding.UTF8);
        var policy = new XmlDocument();
        policy.LoadXml(policyFile);

        var serializer = new Xacml30ProtocolSerializer();

        XacmlContextRequest requestData;

        using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml)))
        {
            requestData = serializer.ReadContextRequest(reader);
        }
        
        EvaluationEngine engine = EvaluationEngineFactory.Create(policy, null);

        XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

    }
}