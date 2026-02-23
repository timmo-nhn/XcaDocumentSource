using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http.Extensions;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.DenyBuilder;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;
using XcaXds.WebService.Services;
using Task = System.Threading.Tasks.Task;


namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint;

public class PolicyEnforcementPointMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddleware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;
    private readonly MonitoringStatusService _monitoringService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RequestThrottlingService _requestThrottlingService;
    private readonly PolicyRepositoryService _policyRepositoryService;

    private static readonly ActivitySource ActivitySource = new("nhn.xcads");
    private static readonly Meter Meter = new("nhn.xcads", "1.0.0");
    private static readonly Counter<int> PepInvokeCounter = Meter.CreateCounter<int>("PolicyEnforcementPoint", description: "Counts the number of PEP invokes");

    public PolicyEnforcementPointMiddleware(
        RequestDelegate next,
        ILogger<PolicyEnforcementPointMiddleware> logger,
        ApplicationConfig xdsConfig,
        IWebHostEnvironment env,
        MonitoringStatusService monitoringService,
        RegistryWrapper registryWrapper,
        RequestThrottlingService requestThrottlingService,
        PolicyRepositoryService policyRepositoryService)
    {
        Console.WriteLine("PEP constructed");
        _next = next;
        _logger = logger;
        _xdsConfig = xdsConfig;
        _env = env;
        _monitoringService = monitoringService;
        _registryWrapper = registryWrapper;
        _requestThrottlingService = requestThrottlingService;
        _policyRepositoryService = policyRepositoryService;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        PolicyInputBuilder policyInputBuilder,
        PolicyEvaluator policyEvaluator,
        PolicyDenyResponseBuilder policyDenyResponseBuilder)
    {
        var sw = Stopwatch.StartNew();

        ThrottleRequestIfRequestThrottlingEnabled(out var millis);

        if (millis > 0)
        {
            _logger.LogWarning($"Requesth throttling enabled: {millis} ms");
        }

        if (!IsPolicyEnforcementPointEnabledForRequestEndpoint(httpContext))
        {
            sw.Stop();
            _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point not enabled for this endpoint");
            await _next(httpContext);
            return;
        }

        var requestUrl = httpContext.Request.GetDisplayUrl();
        var requestMethod = httpContext.Request.Method;
        _logger.LogInformation($"{requestMethod} Request to endpoint: {requestUrl}");

        using var activity = StartPepActivity(httpContext);

        var policyInput = await policyInputBuilder.BuildAsync(httpContext, _xdsConfig, _registryWrapper.GetDocumentRegistryContentAsDtos());

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            LogJwt(httpContext);

            var xacmlPolicySet = XacmlSerializer.SerializeXacmlToXml(_policyRepositoryService.GetPoliciesAsXacmlPolicySet(policyInput.AppliesTo), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            var xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(policyInput.XacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            _logger.LogDebug($"{httpContext.TraceIdentifier} - XACML request:\n{xacmlRequestString}");
        }

        AttachPepContext(httpContext, policyInput.XacmlRequest, sw.ElapsedMilliseconds);

        if (ShouldBypassPolicyEnforcementPoint(httpContext, _xdsConfig, _env))
        {
            if (_xdsConfig.BypassPolicyEnforcementPoint)
            {
                _logger.LogWarning($"{httpContext.TraceIdentifier} - BypassPolicyEnforcementPoint is true!");
            }

            sw.Stop();
            _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point middleware was bypassed");
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Bypassed PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");

            await _next(httpContext);
            return;
        }

        PepInvokeCounter.Add(1);

        if (policyInput.IsSuccess == false)
        {
            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");

            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:tokeninvalid", sw.ElapsedMilliseconds);
            await policyDenyResponseBuilder.WriteAsync(httpContext, policyInput, _xdsConfig, policyInput.ErrorMessage);
            return;
        }

        _logger.LogDebug($"XACML Request:{XacmlSerializer.SerializeXacmlToXml(policyInput.XacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings)}");

        var decision = policyEvaluator.Evaluate(policyInput.XacmlRequest, policyInput.AppliesTo);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point result: {decision.Response.Results.FirstOrDefault()?.Decision.ToString()}");

        if (decision.Permit)
        {
            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");

            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:permit", sw.ElapsedMilliseconds);
            activity?.SetTag("PolicyEnforcementPoint.Status", "permit");

            await _next(httpContext);
            return;
        }

        sw.Stop();
        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point has denied the request");

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
        await policyDenyResponseBuilder.WriteAsync(httpContext, policyInput, _xdsConfig, "Access denied");
        _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:deny", sw.ElapsedMilliseconds);
        activity?.SetTag("PolicyEnforcementPoint.Status", "deny");
    }

    private bool IsPolicyEnforcementPointEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();
        return enforceAttr?.Enabled == true;
    }

    private void AttachPepContext(HttpContext httpContext, XacmlContextRequest? xacmlRequest, long elapsedMillis)
    {
        httpContext.Items.Add("xacmlRequest", xacmlRequest);
        httpContext.Items.Add("pepElapsedTime", elapsedMillis);
    }

    private void ThrottleRequestIfRequestThrottlingEnabled(out int millis)
    {
        millis = 0;

        if (!_requestThrottlingService.IsThrottleTimeSet())
        {
            return;
        }

        var throttleTime = _requestThrottlingService.GetThrottleTime();
        millis = throttleTime;
        Thread.Sleep(throttleTime);
    }

    private Activity? StartPepActivity(HttpContext ctx)
    {
        var activity = ActivitySource.StartActivity("PolicyEnforcementPoint");
        activity?.SetTag("Request.SessionId", ctx.TraceIdentifier);
        PepInvokeCounter.Add(1);
        return activity;
    }

    private bool ShouldBypassPolicyEnforcementPoint(HttpContext context, ApplicationConfig config, IWebHostEnvironment env)
    {
        if (config.BypassPolicyEnforcementPoint)
            return true;

        var isLocal = context.Connection.RemoteIpAddress is { } ip &&
                      (IPAddress.IsLoopback(ip) || ip.ToString() == "::1");

        if (isLocal && config.IgnorePEPForLocalhostRequests && env.IsDevelopment())
            return true;

        return false;
    }

    public void LogJwt(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            _logger.LogWarning("No Bearer token found.");
            return;
        }

        var tokenString = authHeader.Substring("Bearer ".Length).Trim();

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(tokenString))
        {
            _logger.LogWarning("Invalid JWT format.");
            return;
        }

        var jwt = handler.ReadJwtToken(tokenString);

        var jwtObject = new
        {
            Header = jwt.Header,
            Payload = jwt.Payload
        };

        var json = JsonSerializer.Serialize(jwtObject, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _logger.LogInformation("JWT Content:\n{JwtJson}", json);
    }

}

