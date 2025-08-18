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
using XcaXds.Commons.Serializers;
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
                xacmlRequest = await _policyAuthorizationService.GetXacml20RequestFromSoapEnvelope(httpContent);
                break;

            case "application/json":
                xacmlRequest = await _policyAuthorizationService.GetXacml20RequestFromJsonWebToken(httpContent);
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

