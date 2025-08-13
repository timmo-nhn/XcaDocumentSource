using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;
using Hl7.Fhir.Model.CdsHooks;
using PdfSharp.Charting;
using System.Text;
using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Services;
using XcaXds.WebService.Middleware;
using XcaXds.WebService.Services;
using static XcaXds.Commons.Constants.Xacml;

namespace XcaXds.UnitTests;

public class UnitTests_PolicyAuthorization
{
    [Fact]
    public async Task Authorization_PolicyStuff()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData","Policies"));
        var requests = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Tests", "TestData"));

        var policyService = new PolicyAuthorizationService();
        
        XacmlContextRequest xacmlObject = await policyService.GetXacmlRequestFromSoapEnvelope(File.ReadAllText(requests.FirstOrDefault(f => f.Contains("iti18")), Encoding.UTF8));
        var requestXml = XacmlSerializer.SerializeRequestToXml(xacmlObject);
        var requestDoc = new XmlDocument();
        requestDoc.LoadXml(requestXml);

        XacmlPolicy policy01;

        var policyFile = File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("Policy01")), Encoding.UTF8);

        using (XmlReader reader = XmlReader.Create(new StringReader(policyFile)))
        {
            var serialize = new Xacml30ProtocolSerializer();
            policy01 = serialize.ReadPolicy(reader);
        }

        var ngin = new EvaluationEngine30(policy01);

        var gobb = ngin.Evaluate(xacmlObject, requestDoc);

        var jsoncontextresponse = JsonSerializer.Serialize(gobb);
    }
}