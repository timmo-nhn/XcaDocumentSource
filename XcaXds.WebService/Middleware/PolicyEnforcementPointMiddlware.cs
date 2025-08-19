using System.Diagnostics;
using System.Net;
using System.Xml;
using Abc.Xacml;
using Abc.Xacml.Context;
using Hl7.Fhir.Model.CdsHooks;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Middleware;

public class PolicyEnforcementPointMiddlware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddlware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;


    public PolicyEnforcementPointMiddlware(
        RequestDelegate next, 
        ILogger<PolicyEnforcementPointMiddlware> logger,
        ApplicationConfig xdsConfig,
        IWebHostEnvironment env,
        PolicyRepositoryWrapper policyRepositoryWrapper
        )
    {
        _logger = logger;
        _next = next;
        _xdsConfig = xdsConfig;
        _env = env;
        _policyRepositoryWrapper = policyRepositoryWrapper;
    }


    public async Task InvokeAsync(HttpContext httpContext)
    {
        Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "Warning! 'PEP bypass for local requests' is enabled in production!");

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


        var requestBody = await GetHttpRequestBody(httpContext.Request);

        var contentType = httpContext.Request.ContentType?.Split(";").First();
        
        XacmlContextRequest? xacmlRequest = null;

        switch (contentType)
        {
            case "application/soap+xml":
                xacmlRequest = await PolicyRequestMapperService.GetXacml20RequestFromSoapEnvelope(requestBody);
                break;

            case "application/json":
                xacmlRequest = await PolicyRequestMapperService.GetXacml20RequestFromJsonWebToken(requestBody);
                break;
        }


        var request = XacmlSerializer.SerializeRequestToXml(xacmlRequest);

        var serializer = new Xacml20ProtocolSerializer();

        XacmlContextRequest requestData;

        try
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(request)))
            {
                requestData = serializer.ReadContextRequest(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }


        var evaluateResponse = _policyRepositoryWrapper.EvaluateReqeust_V20(xacmlRequest);

        if (evaluateResponse != null && evaluateResponse.Results.All(res => res.Decision == XacmlContextDecision.Permit))
        {
            httpContext.Response.StatusCode = int.Parse(HttpStatusCode.Unauthorized.ToString());
            await httpContext.Response.WriteAsync("Unauthorized access.");
            return;

        }

        // Call the next delegate/middleware in the pipeline.
        await _next(httpContext);
    }

    public static async Task<string> GetHttpRequestBody(HttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0; // Reset stream position for next reader
        return bodyContent;
    }
}

