using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml;
using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Services;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Middleware;

public class PolicyEnforcementPointMiddlware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddlware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;
    private readonly PolicyAuthorizationService _policyAuthorizationService;

    public PolicyEnforcementPointMiddlware(RequestDelegate next, ILogger<PolicyEnforcementPointMiddlware> logger, ApplicationConfig xdsConfig, IWebHostEnvironment env, PolicyAuthorizationService policyAuthorizationService)
    {
        _logger = logger;
        _next = next;
        _xdsConfig = xdsConfig;
        _env = env;
        _policyAuthorizationService = policyAuthorizationService;
    }


    public async Task InvokeAsync(HttpContext httpContext)
    {
        Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "Warning! PEP bypass is enabled in production!");

        // If the request is from localhost and environment is development we can ignore PEP.
        var requestIsLocal = httpContext.Connection.RemoteIpAddress != null &&
              (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress) ||
               httpContext.Connection.RemoteIpAddress.ToString() == "::1");

        if (requestIsLocal &&
            _xdsConfig.IgnorePEPForLocalhostRequests == true &&
            _env.IsDevelopment() == true)
        {
            _logger.LogWarning("Policy Enforcement Point middleware was bypassed for requests from localhost.");
            await _next(httpContext);
            return;
        }

        _logger.LogInformation("Request Content-type: " + httpContext.Request.ContentType);

        var endpoint = httpContext.GetEndpoint();
        var enforceAttr = endpoint?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();

        if (enforceAttr == null || enforceAttr.Enabled == false)
        {
            await _next(httpContext); // Skip PEP check
            return;
        }

        httpContext.Request.EnableBuffering(); // Allows multiple reads

        XacmlContextRequest? xacmlRequest = null;

        var httpContent = await GetBodyContentFromHttpRequest(httpContext.Request);

        var contentType = httpContext.Request.ContentType?.Split(";").First();

        if (string.IsNullOrWhiteSpace(httpContent))
        {
            await _next(httpContext);
        }

        switch (contentType)
        {
            case "application/soap+xml":
                xacmlRequest = await _policyAuthorizationService.GetXacml30RequestFromSoapEnvelope(httpContent);
                break;

            case "application/json":
                xacmlRequest = await _policyAuthorizationService.GetXacmlRequestFromJsonWebToken(httpContent);
                break;
        }

        if (xacmlRequest == null)
        {
            await _next(httpContext);
        }

        var request = XacmlSerializer.SerializeRequestToXml(xacmlRequest);

        var serializer = new Xacml30ProtocolSerializer();

        XacmlContextRequest requestData;

        using (XmlReader reader = XmlReader.Create(new StringReader(request)))
        {
            requestData = serializer.ReadContextRequest(reader);
        }

        var policyRepository = new PolicyRepository();

        //policyRepository.RequestPolicy(new Uri(""));

        //var policy = new XmlDocument();
        //policy.LoadXml(policyFile);


        //EvaluationEngine engine = EvaluationEngineFactory.Create(policy,);

        //XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);


        // Call the next delegate/middleware in the pipeline.
        await _next(httpContext);
    }

    public static async Task<string> GetBodyContentFromHttpRequest(HttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0; // Reset stream position for next reader
        return bodyContent;
    }
}

public static class XacmlSerializer
{
    public static string? SerializeRequestToXml(XacmlContextRequest request)
    {
        if (request == null) return null;

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

        xmlWriter.WriteAttributeString("ReturnPolicyIdList", request.ReturnPolicyIdList.ToString().ToLower());
        xmlWriter.WriteAttributeString("CombinedDecision", request.CombinedDecision.ToString().ToLower());

        if (request.Subjects.Count == 0)
        {
            var subjectAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("subject")));
            WriteCategoryAttributes(xmlWriter, subjectAttributes, Constants.Xacml.Category.Subject);
        }
        else
        {
            WriteSubject(xmlWriter, request.Subjects);
        }

        if (request.Resources.Count == 0)
        {
            var resourceAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("resource-id")));
            WriteCategoryAttributes(xmlWriter, resourceAttributes, Constants.Xacml.Category.Resource);
        }
        else
        {
            WriteResources(xmlWriter, request.Resources);
        }

        if (request.Action == null)
        {
            var actionAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("action-id")));
            WriteCategoryAttributes(xmlWriter, actionAttributes, Constants.Xacml.Category.Action);
        }
        else
        {
            WriteAction(xmlWriter, request.Action);
        }

        if (request.Environment == null)
        {
            var environmentAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("environment")));
            WriteCategoryAttributes(xmlWriter, environmentAttributes, Constants.Xacml.Category.Environment);

        }
        else
        {
            WriteEnvironment(xmlWriter, request.Environment);
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();

        var serializedRequest = stringWriter.ToString();
        serializedRequest = serializedRequest.Replace("&amp;amp;", "&amp;");

        return serializedRequest;
    }


    private static void WriteCategoryAttributes(XmlWriter xmlWriter, IEnumerable<XacmlContextAttributes> categoryAttributes, string category)
    {
        if (categoryAttributes.Count() == 0) return;

        foreach (var attribute in categoryAttributes)
        {
            xmlWriter.WriteStartElement("Attributes");
            xmlWriter.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
            xmlWriter.WriteAttributeString("Category", category);
            foreach (var attr in attribute.Attributes)
            {
                WriteAttribute(xmlWriter, attr);
            }
            xmlWriter.WriteEndElement();
        }
    }

    private static void WriteSubject(XmlWriter xmlWriter, ICollection<XacmlContextSubject> subjects)
    {
        if (subjects == null || subjects.Count() == 0) return;

        foreach (var subject in subjects)
        {
            xmlWriter.WriteStartElement("Attributes");
            xmlWriter.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
            xmlWriter.WriteAttributeString("Category", Constants.Xacml.Category.Subject);
            foreach (var attr in subject.Attributes)
            {
                WriteContextAttribute(xmlWriter, attr);
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
            writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
            writer.WriteAttributeString("Category", Constants.Xacml.Category.Resource);

            foreach (var attr in resource.Attributes)
                WriteContextAttribute(writer, attr);

            writer.WriteEndElement();
        }
    }

    private static void WriteAction(XmlWriter writer, XacmlContextAction action)
    {
        if (action == null) return;


        writer.WriteStartElement("Attributes");
        writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
        writer.WriteAttributeString("Category", Constants.Xacml.Category.Action);

        foreach (var attr in action.Attributes)
            WriteContextAttribute(writer, attr);

        writer.WriteEndElement();

    }

    private static void WriteEnvironment(XmlWriter writer, XacmlContextEnvironment environment)
    {
        if (environment == null) return;

        writer.WriteStartElement("Attributes");
        writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
        writer.WriteAttributeString("Category", Constants.Xacml.Category.Environment);

        foreach (var attr in environment.Attributes)
            WriteContextAttribute(writer, attr);

        writer.WriteEndElement();
    }

    private static void WriteContextAttribute(XmlWriter writer, XacmlContextAttribute attr)
    {
        if (attr == null) return;

        writer.WriteStartElement("Attribute");
        writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
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

    private static void WriteAttribute(XmlWriter writer, XacmlAttribute attr)
    {
        if (attr == null) return;

        writer.WriteStartElement("Attribute");
        writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
        writer.WriteAttributeString("AttributeId", attr.AttributeId.ToString());

        foreach (var val in attr.AttributeValues)
        {
            writer.WriteStartElement("AttributeValue");
            writer.WriteAttributeString("DataType", val.DataType.ToString());
            writer.WriteString(val.Value);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }
}