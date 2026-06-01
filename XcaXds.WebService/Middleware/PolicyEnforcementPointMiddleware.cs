using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using XcaXds.Commons.Attributes;
using XcaXds.Commons.Commons;
using XcaXds.WebService.Services;
using XcaXds.WebService.Services.PolicyEnforcementPoint;
using XcaXds.WebService.Services.PolicyEnforcementPoint.DenyBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint.InputBuilder;
using Task = System.Threading.Tasks.Task;


namespace XcaXds.WebService.Middleware;

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
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;
    private readonly PolicyDecisionPointService _policyDecisionPointService;

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
        PolicyRepositoryService policyRepositoryService,
        PolicyRepositoryWrapper policyRepositoryWrapper,
        PolicyDecisionPointService policyDecisionPointService)
    {
        _next = next;
        _logger = logger;
        _xdsConfig = xdsConfig;
        _env = env;
        _monitoringService = monitoringService;
        _registryWrapper = registryWrapper;
        _requestThrottlingService = requestThrottlingService;
        _policyRepositoryService = policyRepositoryService;
        _policyRepositoryWrapper = policyRepositoryWrapper;
        _policyDecisionPointService = policyDecisionPointService;;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        PolicyInputBuilder policyInputBuilder,
        PolicyDenyResponseBuilder policyDenyResponseBuilder)
    {
        var sw = Stopwatch.StartNew();

        ThrottleRequestIfRequestThrottlingEnabled(out var millis);

        if (millis > 0)
        {
            _logger.LogWarning($"Requesth throttling enabled: {millis} ms");
        }

        var requestUrl = httpContext.Request.GetDisplayUrl();
        var requestMethod = httpContext.Request.Method;
        _logger.LogInformation($"{httpContext.TraceIdentifier} - {requestMethod} Request to endpoint: {requestUrl}");

        if (!PolicyEnforcementPointEnabledForRequestEndpoint(httpContext))
        {
            sw.Stop();
            _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point not enabled for this endpoint");
            await _next(httpContext);
            return;
        }

        using var activity = StartPepActivity(httpContext);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Beginning policy input builder...");
        var policyInput = await policyInputBuilder.BuildAsync(httpContext, _xdsConfig);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy input builder complete. Success: {policyInput.IsSuccess}, Message: {policyInput.ErrorMessage}");

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            LogJwt(httpContext);
            var policies = _policyRepositoryService.GetPoliciesAsPolicySetDto();
            var policySet = JsonSerializer.Serialize(policies, Constants.JsonDefaultOptions.DefaultSettings);
            var accessControlRequestString = JsonSerializer.Serialize(policyInput.AccessRequest, Constants.JsonDefaultOptions.DefaultSettings);
            _logger.LogDebug($"{httpContext.TraceIdentifier} - ABAC request:\n{accessControlRequestString}");
        }

        AttachPepContext(httpContext, policyInput.AccessRequest, sw.ElapsedMilliseconds);

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
        
        var decision = _policyDecisionPointService.Evaluate(policyInput.AccessRequest);
        _logger.LogInformation(JsonSerializer.Serialize(decision, Constants.JsonDefaultOptions.DefaultSettings));
        
        AttachPepDecisionResponse(httpContext, decision);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point result: {decision.Decision.ToString()}");

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

    private void AttachPepDecisionResponse(HttpContext httpContext, AccessControlResponse decision)
    {
        httpContext.Items.Add("pdpDecision",decision);
    }

    private bool PolicyEnforcementPointEnabledForRequestEndpoint(HttpContext httpContext)
    {
        var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();
        return enforceAttr?.Enabled == true;
    }

    private void AttachPepContext(HttpContext httpContext, AbacRequest? accessRequest, long elapsedMillis)
    {
        httpContext.Items.Add("accessRequest", accessRequest);
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
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

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