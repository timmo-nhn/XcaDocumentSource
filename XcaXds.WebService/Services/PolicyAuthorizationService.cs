using Abc.Xacml.Context;
using Hl7.Fhir.Utility;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Services;

public class PolicyAuthorizationService
{
    private readonly ILogger<PolicyAuthorizationService> _logger;
    public PolicyAuthorizationService(ILogger<PolicyAuthorizationService> logger)
    {
        _logger = logger;
    }

    public PolicyAuthorizationService()
    {
        
    }

    public async Task<XacmlContextRequest?> GetXacmlRequestFromJsonRequest(HttpContext httpContext)
    {
        XacmlContextRequest contextRequest = null;
        return contextRequest;
    }

     public async Task<XacmlContextRequest> GetXacmlRequestFromSoapEnvelope(HttpContext httpContext)
    {
        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpContext.Request.Body.Position = 0; // Reset stream position for next reader
        return await GetXacmlRequestFromSoapEnvelope(bodyContent);
    }

    public async Task<XacmlContextRequest> GetXacmlRequestFromSoapEnvelope(string inputSoapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var soapEnvelope = new XmlDocument();
        soapEnvelope.LoadXml(inputSoapEnvelope);

        var soapEnvelopeObject = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(inputSoapEnvelope);

        var assertion = soapEnvelope.GetElementsByTagName("saml:Assertion");

        if (assertion.Count == 0)
        {
            if (_logger != null)
            {
                _logger.LogDebug("No saml token in request");
            }
            throw new Exception("No SAML Assertion found in the SOAP envelope.");
        }

        var samlAssertion = assertion[0]?.OuterXml;

        var handler = new Saml2SecurityTokenHandler();
        var samlToken = handler.ReadSaml2Token(samlAssertion);

        var authStatement = samlToken.Assertion.Statements.OfType<Saml2AuthenticationStatement>().FirstOrDefault();
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var samltokenAuthorizationAttributes = statements.Where(att => att.Name.Contains("xacml"));

        

        var subjectAttributes = new List<XacmlContextAttribute>();

        foreach (var attribute in samltokenAuthorizationAttributes)
        {
            foreach (var attributeValue in attribute.Values)
            {
                var contextAttributeValues = new XacmlContextAttributeValue();

                try
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(Regex.Replace(attributeValue, @"\bxsi:\b", ""));

                    var gobb = xmlDocument.ChildNodes[0].Attributes.GetNamedItem("type").Value;

                    //switch ()
                    //{
                    //    default:
                    //        break;
                    //}

                }
                catch (Exception)
                {

                    continue;
                }

                subjectAttributes.Add(new XacmlContextAttribute(
                    new Uri(attribute.Name), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = attributeValue }));
            }
        }

        // Resource
        var xacmlResourceAttribute = subjectAttributes.FirstOrDefault(sa => sa.AttributeId.OriginalString.Contains("resource-id"));

        var resourceId = Hl7Object.Parse<CX>(xacmlResourceAttribute?.AttributeValues.FirstOrDefault()?.Value);

        var xacmlResource = new XacmlContextResource(xacmlResourceAttribute);

        // Action
        var action = string.Empty;
        switch (soapEnvelopeObject.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti42Action:
            case Constants.Xds.OperationContract.Iti18Action:
                action = "read";
                break;

            case Constants.Xds.OperationContract.Iti41Action:
                action = "write";
                break;

            case Constants.Xds.OperationContract.Iti62Action:
            case Constants.Xds.OperationContract.Iti86Action:
                action = "delete";
                break;

            default:
                break;
        }

        var actionAttribute = new XacmlContextAttribute(
            new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = action });

        var xacmlAction = new XacmlContextAction(actionAttribute);

        // Subject
        var xacmlSubject = new XacmlContextSubject(subjectAttributes);


        // Environment
        var xacmlEnvironment = new XacmlContextEnvironment();

        var request = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        return request;
    }
}
