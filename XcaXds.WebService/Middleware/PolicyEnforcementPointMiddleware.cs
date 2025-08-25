using Abc.Xacml.Context;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Diagnostics;
using System.Net;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;

namespace XcaXds.WebService.Middleware;

public class PolicyEnforcementPointMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddleware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;
    private readonly PolicyRepositoryService _debug_policyRepositoryService;
    private readonly PolicyDecisionPointService _policyDecisionPointService;


    public PolicyEnforcementPointMiddleware(
        RequestDelegate next,
        ILogger<PolicyEnforcementPointMiddleware> logger,
        ApplicationConfig xdsConfig,
        IWebHostEnvironment env,
        PolicyRepositoryService debug_policyRepositoryService,
        PolicyDecisionPointService policyDecisionPointService
        )
    {
        _logger = logger;
        _next = next;
        _xdsConfig = xdsConfig;
        _env = env;
        _debug_policyRepositoryService = debug_policyRepositoryService;
        _policyDecisionPointService = policyDecisionPointService;
    }


    public async Task InvokeAsync(HttpContext httpContext)
    {
        Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "Warning! 'PEP bypass for local requests' is enabled in production!");

        // If the request is from localhost and environment is development we can ignore PEP.
        var requestIsLocal = httpContext.Connection.RemoteIpAddress != null &&
              (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress) ||
               httpContext.Connection.RemoteIpAddress.ToString() == "::1");

        if (requestIsLocal && _xdsConfig.IgnorePEPForLocalhostRequests == true && _env.IsDevelopment() == true)
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

                var xacmlAction = PolicyRequestMapperSamlService.MapXacmlActionFromSoapAction(PolicyRequestMapperSamlService.GetActionFromSoapEnvelope(requestBody));
                var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(PolicyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody));
                xacmlRequest = await PolicyRequestMapperSamlService.GetXacmlRequestFromSamlToken(samlToken, xacmlAction, XacmlVersion.Version20);

                break;

            case "application/json":
                xacmlRequest = await PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(requestBody);
                
                break;
        }

        string xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(xacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var policySetXml = XacmlSerializer.SerializeXacmlToXml(_debug_policyRepositoryService.GetPoliciesAsXacmlPolicySet(), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var evaluateResponse = _policyDecisionPointService.EvaluateRequest(xacmlRequest);

        if (evaluateResponse != null && evaluateResponse.Results.All(res => res.Decision == XacmlContextDecision.Permit))
        {
            await _next(httpContext);
        }

        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return;

    }

    public static async Task<string> GetHttpRequestBody(HttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0; // Reset stream position for next reader
        return bodyContent;
    }
}

