using Abc.Xacml.Context;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Middleware;

public class PolicyEnforcementPointMiddlware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddlware> _logger;
    private readonly XdsConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;

    public PolicyEnforcementPointMiddlware(RequestDelegate next, ILogger<PolicyEnforcementPointMiddlware> logger, XdsConfig xdsConfig, IWebHostEnvironment env)
    {
        _logger = logger;
        _next = next;
        _xdsConfig = xdsConfig;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "PEP bypass is enabled in production!");
        
        // If the request is from localhost and environment is development we can ignore PEP.
        var requestIsLocal = httpContext.Connection.RemoteIpAddress is not null &&
              (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress) ||
               httpContext.Connection.RemoteIpAddress.ToString() == "::1");

        if (requestIsLocal &&
            _xdsConfig.IgnorePEPForLocalhostRequests is true &&
            _env.IsDevelopment() is true)
        {
            _logger.LogWarning("Policy Enforcement Point middleware was bypassed for requests from localhost.");
            await _next(httpContext);
            return;
        }

        var endpoint = httpContext.GetEndpoint();
        var enforceAttr = endpoint?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();

        if (enforceAttr is null || enforceAttr.Enabled is false)
        {
            await _next(httpContext); // Skip PEP check
            return;
        }

        if (httpContext.Request.ContentType.Contains("application/soap+xml") is false)
        {
            await _next(httpContext);
            return;
        }

        httpContext.Request.EnableBuffering(); // Allows multiple reads
        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();

        httpContext.Request.Body.Position = 0; // Reset stream position for next reader
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var soapEnvelope = new XmlDocument();
        soapEnvelope.LoadXml(bodyContent);

        var soapEnvelopeObject = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(bodyContent);

        var assertion = soapEnvelope.GetElementsByTagName("saml:Assertion");

        var ip = httpContext.Connection.RemoteIpAddress.ToString();


        if (assertion.Count == 0)
        {
            _logger.LogDebug("No saml token in request");
            throw new Exception("No SAML Assertion found in the SOAP envelope.");
        }

        var samlAssertion = assertion[0].OuterXml;

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
                subjectAttributes.Add(new XacmlContextAttribute(
                    new Uri(attribute.Name), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = attributeValue }));
            }
        }

        // Resource

        var xacmlResourceAttribute = subjectAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id")).FirstOrDefault();

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


        var xacmlSubject = new XacmlContextSubject(subjectAttributes);
        var request = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        var xacmlRequestString = XacmlSerializer.SerializeRequest(request);

        /* 
         * Call PDP Endpoint with request string here
         * short circuit middleware and return Soap response if response from PDP is Deny
        */

        // Example where PEP denied the request; short-circuit the middleware

        if ("pepdenied".Equals("true"))
        {
            var responseEnvelope = SoapExtensions.CreateSoapFault("Access denied").Value;

            _logger.LogError($"Access denied from Policy Decision Point \n Reason: {responseEnvelope?.Body.Fault?.Code.Value}");
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            httpContext.Response.ContentType = "application/soap+xml";
            await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(responseEnvelope).Content);
            return;
        }


        // Call the next delegate/middleware in the pipeline.
        await _next(httpContext);
    }
}

public static class XacmlSerializer
{
    public static string SerializeRequest(XacmlContextRequest request)
    {
        var settings = new XmlWriterSettings()
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("Request", Constants.Xacml.Namespace.WD17);

        WriteSubject(xmlWriter, request.Subjects);
        WriteResources(xmlWriter, request.Resources);
        WriteAction(xmlWriter, request.Action);
        WriteEnvironment(xmlWriter, request.Environment);

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();

        var serializedRequest = stringWriter.ToString();
        serializedRequest = serializedRequest.Replace("&amp;amp;", "&amp;");

        return serializedRequest;
    }

    private static void WriteSubject(XmlWriter xmlWriter, ICollection<XacmlContextSubject> subjects)
    {
        if (subjects == null || subjects.Count() == 0) return;

        foreach (var subject in subjects)
        {
            xmlWriter.WriteStartElement("Attributes");
            xmlWriter.WriteAttributeString("Category", Constants.Xacml.Category.Subject);
            foreach (var attr in subject.Attributes)
            {
                WriteAttribute(xmlWriter, attr);
            }
            xmlWriter.WriteEndElement();
        }
    }

    private static void WriteResources(XmlWriter writer, IEnumerable<XacmlContextResource> resources)
    {
        if (resources == null || resources.Count() == 0) return;

        foreach (var resource in resources)
        {
            writer.WriteStartElement("Attributes");
            writer.WriteAttributeString("Category", Constants.Xacml.Category.Resource);

            foreach (var attr in resource.Attributes)
                WriteAttribute(writer, attr);

            writer.WriteEndElement();
        }
    }

    private static void WriteAction(XmlWriter writer, XacmlContextAction action)
    {
        if (action == null) return;


        writer.WriteStartElement("Attributes");
        writer.WriteAttributeString("Category", Constants.Xacml.Category.Action);

        foreach (var attr in action.Attributes)
            WriteAttribute(writer, attr);

        writer.WriteEndElement();

    }

    private static void WriteEnvironment(XmlWriter writer, XacmlContextEnvironment environment)
    {
        if (environment == null) return;

        writer.WriteStartElement("Attributes");
        writer.WriteAttributeString("Category", Constants.Xacml.Category.Environment);

        foreach (var attr in environment.Attributes)
            WriteAttribute(writer, attr);

        writer.WriteEndElement();
    }
    private static void WriteAttribute(XmlWriter writer, XacmlContextAttribute attr)
    {
        if (attr == null) return;

        writer.WriteStartElement("Attribute");
        writer.WriteAttributeString("AttributeId", attr.AttributeId.ToString());
        //writer.WriteAttributeString("IncludeInResult", attr..ToString().ToLower());


        foreach (var val in attr.AttributeValues)
        {
            writer.WriteStartElement("AttributeValue");
            writer.WriteAttributeString("DataType", attr.DataType.ToString());
            writer.WriteString(val.Value);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

}


public static class PolicyEnforcementPointMiddlewareExtensions
{
    public static IApplicationBuilder UsePolicyEnforcementPointMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PolicyEnforcementPointMiddlware>();
    }
}