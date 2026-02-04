using Hl7.Fhir.Model;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
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

        if (ShouldBypassPolicyEnforcementPoint(httpContext, _xdsConfig, _env))
        {
            sw.Stop();
            _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point middleware was bypassed");
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Bypassed PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");

            await _next(httpContext);
            return;
        }

        using var activity = StartPepActivity(httpContext);

        PepInvokeCounter.Add(1);

        var policyInput = await policyInputBuilder.BuildAsync(httpContext, _xdsConfig, _registryWrapper.GetDocumentRegistryContentAsDtos());

        if (policyInput.IsSuccess == false)
        {
            sw.Stop();
            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:tokeninvalid", sw.ElapsedMilliseconds);
            await policyDenyResponseBuilder.WriteAsync(httpContext, policyInput, _xdsConfig, policyInput.ErrorMessage);
            return;
        }

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            var xacmlPolicySet = XacmlSerializer.SerializeXacmlToXml(_policyRepositoryService.GetPoliciesAsXacmlPolicySet(policyInput.AppliesTo), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            var xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(policyInput.XacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            _logger.LogDebug($"{httpContext.TraceIdentifier} - XACML request:\n{xacmlRequestString}");
        }


        var decision = policyEvaluator.Evaluate(policyInput.XacmlRequest, policyInput.AppliesTo);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point result: {decision.Response.Results.FirstOrDefault()?.Decision.ToString()}");
        if (decision.Permit)
        {
            sw.Stop();
            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:permit", sw.ElapsedMilliseconds);
            activity?.SetTag("PolicyEnforcementPoint.Status", "permit");

            AttachPepContext(httpContext, policyInput, sw.ElapsedMilliseconds);
            await _next(httpContext);
            return;
        }

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point has denied the request");
        await policyDenyResponseBuilder.WriteAsync(httpContext, policyInput, _xdsConfig, "Access denied");
        _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:deny", sw.ElapsedMilliseconds);
        activity?.SetTag("PolicyEnforcementPoint.Status", "deny");
    }

    private void AttachPepContext(HttpContext httpContext, PolicyInputResult policyInput, long elapsedMillis)
    {
        httpContext.Items.Add("xacmlRequest", policyInput.XacmlRequest);
        httpContext.Items.Add("pepElapsedTime", elapsedMillis);
    }

    //public async Task InvokeAsync(HttpContext httpContext)
    //{
    //    ThrottleRequestIfRequestThrottlingEnabled(out var millis);
    //    if (millis > 0)
    //    {
    //        _logger.LogWarning($"Requesth throttling enabled: {millis} ms");
    //    }

    //    #region Determine if PEP should be used for this request

    //    if (_xdsConfig.BypassPolicyEnforcementPoint)
    //    {
    //        _logger.LogWarning("BypassPolicyEnforcementPoint is true! Policy Enforcement Point was bypassed.");
    //        await _next(httpContext); // Skip PEP check
    //        return;
    //    }

    //    Stopwatch sw = Stopwatch.StartNew();
    //    Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "Warning! 'PEP bypass for local requests' is enabled in production!");

    //    // If the request is from localhost and environment is development we can ignore PEP.
    //    var requestIsLocal = httpContext.Connection.RemoteIpAddress != null &&
    //          (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress) ||
    //           httpContext.Connection.RemoteIpAddress.ToString() == "::1");

    //    if (requestIsLocal && _xdsConfig.IgnorePEPForLocalhostRequests == true && _env.IsDevelopment() == true)
    //    {
    //        sw.Stop();
    //        _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point middleware was bypassed for requests from localhost in development.");
    //        _logger.LogInformation($"{httpContext.TraceIdentifier} - Bypassed PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
    //        await _next(httpContext); // Skip PEP check
    //        return;
    //    }

    //    var enforceAttr = httpContext.GetEndpoint()?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();

    //    if (enforceAttr == null || enforceAttr.Enabled == false)
    //    {
    //        sw.Stop();
    //        _logger.LogInformation($"Request endpoint doesnt use PEP");
    //        _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
    //        await _next(httpContext); // Skip PEP check
    //        return;
    //    }
    //    #endregion


    //    #region Prepare the request body for processing to XACML-request
    //    // If we are here, the PEP is enabled for the requesting endpoint
    //    using var activity = ActivitySource.StartActivity("PolicyEnforcementPoint");
    //    activity?.SetTag("Request.SessionId", httpContext.TraceIdentifier);
    //    PepInvokeCounter.Add(1);

    //    httpContext.Request.EnableBuffering(); // Allows multiple reads

    //    DebugLogRequestHeaders(httpContext);

    //    var requestBody = await GetRequestBodyFromHttpContext(httpContext);
    //    _logger.LogDebug($"{httpContext.TraceIdentifier} - Request Body:\n{requestBody}");

    //    var contentType = httpContext.Request.ContentType?.Split(";").First();
    //    _logger.LogInformation($"{httpContext.TraceIdentifier} - Request Content-type: {contentType}");

    //    #endregion

    //    bool tokenIsValid = true;
    //    var xacmlRequestAppliesTo = Issuer.Unknown;
    //    XacmlContextRequest? xacmlRequest = null;
    //    RestfulApiResponse? restfulApiResponse = null;



    //    switch (contentType)
    //    {
    //        case Constants.MimeTypes.Json:
    //            var tokenForJsonRequest = JwtExtractor.ExtractJwt(httpContext.Request.Headers, out var jsonSuccess);

    //            if (jsonSuccess == false && tokenForJsonRequest == null)
    //            {
    //                restfulApiResponse ??= new RestfulApiResponse(false, $"Invalid or missing JWT");
    //                goto default;
    //            }

    //            xacmlRequest = PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(tokenForJsonRequest, null, httpContext.Request.Path, httpContext.Request.Method);
    //            break;

    //        case Constants.MimeTypes.FhirJson:
    //            var tokenForFhirRequest = JwtExtractor.ExtractJwt(httpContext.Request.Headers, out var fhirSuccess);

    //            if (fhirSuccess == false && tokenForFhirRequest == null)
    //            {
    //                restfulApiResponse ??= new RestfulApiResponse(false, $"Invalid or missing JWT");
    //                goto default;
    //            }

    //            var fhirJsonDeserializer = new FhirJsonDeserializer();
    //            var fhirBundle = fhirJsonDeserializer.Deserialize<Bundle>(requestBody);

    //            xacmlRequest = PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(tokenForFhirRequest, fhirBundle, httpContext.Request.Path, httpContext.Request.Method);
    //            break;


    //        case Constants.MimeTypes.XopXml:
    //        case Constants.MimeTypes.MultipartRelated:
    //        case Constants.MimeTypes.SoapXml:
    //        case Constants.MimeTypes.Xml:

    //            var samlTokenString = PolicyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

    //            if (!string.IsNullOrEmpty(samlTokenString))
    //            {
    //                if (_xdsConfig.ValidateSamlTokenIntegrity)
    //                {
    //                    var validations = new Saml2SecurityTokenHandler();
    //                    var validator = new Saml2Validator([_xdsConfig.HelseidCert, _xdsConfig.HelsenorgeCert]);

    //                    tokenIsValid = validator.ValidateSamlToken(samlTokenString, out var message);

    //                    if (tokenIsValid == false)
    //                    {
    //                        _logger.LogInformation($"{httpContext.TraceIdentifier} - Invalid SAML-token!");
    //                        _logger.LogInformation($"{httpContext.TraceIdentifier} - {message}");
    //                        break;
    //                    }
    //                }

    //                _logger.LogInformation($"{httpContext.TraceIdentifier} - Saml token is valid!");

    //                var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(requestBody);
    //                var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);

    //                var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Assertion.Issuer.Value);

    //                var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

    //                xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, documentRegistry);

    //                xacmlRequestAppliesTo = appliesTo;
    //            }

    //            if (xacmlRequest == null)
    //            {
    //                var soapResponseEnvelope = new SoapEnvelope();

    //                if (tokenIsValid)
    //                {
    //                    soapResponseEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "No saml token found in SOAP-message").Value;
    //                }
    //                else
    //                {
    //                    soapResponseEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "Invalid saml token in SOAP-message").Value;
    //                }

    //                var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
    //                httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

    //                await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(soapResponseEnvelope).Content ?? string.Empty);

    //                sw.Stop();
    //                _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:tokeninvalid", sw.ElapsedMilliseconds);
    //                activity?.SetTag("PolicyEnforcementPoint.Status", "tokeninvalid");
    //                _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
    //                return;
    //            }
    //            break;

    //        default:

    //            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    //            httpContext.Response.ContentType = Constants.MimeTypes.Json;

    //            restfulApiResponse ??= new RestfulApiResponse(false, $"Unknown or unspecified content type: {httpContext.Request.ContentType}".TrimEnd([':', ' ']));

    //            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(restfulApiResponse, Constants.JsonDefaultOptions.DefaultSettings));

    //            sw.Stop();
    //            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:deny", sw.ElapsedMilliseconds);
    //            activity?.SetTag("PolicyEnforcementPoint.Status", "deny");
    //            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
    //            return;
    //    }


    //    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
    //    {
    //        var xacmlPolicySet = XacmlSerializer.SerializeXacmlToXml(_policyRepositoryService.GetPoliciesAsXacmlPolicySet(xacmlRequestAppliesTo), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
    //        var xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(xacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
    //        _logger.LogDebug($"{httpContext.TraceIdentifier} - XACML request:\n{xacmlRequestString}");
    //    }


    //    var evaluateResponse = _policyDecisionPointService.EvaluateXacmlRequest(xacmlRequest, xacmlRequestAppliesTo);

    //    _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point result: {evaluateResponse.Results.FirstOrDefault()?.Decision.ToString()}");

    //    if (evaluateResponse.Results.All(res => res.Decision == XacmlContextDecision.Permit))
    //    {
    //        sw.Stop();


    //        _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");

    //        _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:permit", sw.ElapsedMilliseconds);

    //        activity?.SetTag("PolicyEnforcementPoint.Status", "permit");

    //        await _next(httpContext);
    //        return;
    //    }

    //    // If the request is permitted, the response generation will naturally be handled by the following middleware components or controller
    //    // If the request has been denied - like it is if we end up here - we need to short circuit the middleware and create an appropriate response ourself!
    //    _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point has denied the request.");

    //    if (contentType == Constants.MimeTypes.XopXml ||
    //        contentType == Constants.MimeTypes.MultipartRelated ||
    //        contentType == Constants.MimeTypes.SoapXml ||
    //        contentType == Constants.MimeTypes.Xml)
    //    {
    //        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

    //        var soapEnvelopeRequest = sxmls.DeserializeXmlString<SoapEnvelope>(requestBody);

    //        var soapEnvelopeResponse = new SoapEnvelope()
    //        {
    //            Header = new()
    //            {
    //                Action = soapEnvelopeRequest.GetCorrespondingResponseAction(),
    //                MessageId = Guid.NewGuid().ToString(),
    //                RelatesTo = soapEnvelopeRequest.Header.MessageId,
    //            },
    //            Body = new()
    //        };

    //        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point has denied the request: id {soapEnvelopeResponse.Header.MessageId}");

    //        var registryResponse = new RegistryResponseType();
    //        registryResponse.AddError(XdsErrorCodes.XDSRegistryError, $"Access denied", _xdsConfig.HomeCommunityId);

    //        SoapExtensions.PutRegistryResponseInTheCorrectPlaceAccordingToSoapAction(soapEnvelopeResponse, registryResponse);

    //        if (contentType == Constants.MimeTypes.XopXml ||
    //            contentType == Constants.MimeTypes.MultipartRelated)
    //        {
    //            var soapMultipart = MultipartExtensions.ConvertRetrieveDocumentSetResponseToMultipartResponse(soapEnvelopeResponse, out var boundary);

    //            var contentId = string.Empty;

    //            if (soapMultipart.FirstOrDefault()?.Headers.TryGetValues("Content-ID", out var contentIdValues) ?? false)
    //            {
    //                contentId = contentIdValues.First().TrimStart('<').TrimEnd('>');
    //            }

    //            httpContext.Response.ContentType = $"{Constants.MimeTypes.MultipartRelated}; type=\"{Constants.MimeTypes.XopXml}\"; boundary=\"{boundary}\"; start=\"{contentId}\"; start-info=\"{Constants.MimeTypes.SoapXml}\"";

    //            var soapByteArray = await MultipartExtensions.SerializeMultipartAsync(soapMultipart);

    //            await httpContext.Response.Body.WriteAsync(soapByteArray);
    //        }
    //        else
    //        {
    //            httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

    //            var soapResponseString = sxmls.SerializeSoapMessageToXmlString(soapEnvelopeResponse).Content;

    //            await httpContext.Response.WriteAsync(soapResponseString ?? string.Empty);
    //        }
    //    }
    //    else if (contentType == Constants.MimeTypes.FhirJson)
    //    {
    //        var fhirJsonSerializer = new FhirJsonSerializer();

    //        httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    //        httpContext.Response.ContentType = Constants.MimeTypes.FhirJson;

    //        var fhirJsonResponse = OperationOutcome.ForMessage("Access denied", OperationOutcome.IssueType.Forbidden, OperationOutcome.IssueSeverity.Error);
    //        await httpContext.Response.WriteAsync(fhirJsonSerializer.SerializeToString(fhirJsonResponse, true));
    //    }
    //    else // if (contentType == Constants.MimeTypes.Json)
    //    {
    //        restfulApiResponse ??= new RestfulApiResponse(false, "Access denied");
    //        httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    //        httpContext.Response.ContentType = Constants.MimeTypes.Json;

    //        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(restfulApiResponse, Constants.JsonDefaultOptions.DefaultSettings));
    //    }

    //    sw.Stop();

    //    _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:deny", sw.ElapsedMilliseconds);
    //    activity?.SetTag("PolicyEnforcementPoint.Status", "deny");

    //    _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");

    //    return;
    //}

    private async Task<string> GetRequestBodyFromHttpContext(HttpContext httpContext)
    {
        var requestBody = await HttpRequestResponseExtensions.GetHttpRequestBodyAsStringAsync(httpContext.Request);

        if (httpContext.Request.Headers.ContentType.Any(ct => ct != null && ct.Contains("boundary")))
        {
            requestBody = (await MultipartExtensions.ReadMultipartContentFromRequest(httpContext.Request)).Trim();
        }

        return requestBody;
    }

    private void DebugLogRequestHeaders(HttpContext httpContext)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{httpContext.TraceIdentifier} - Request headers");
        foreach (var item in httpContext.Request.Headers)
        {
            sb.AppendLine(item.Key + ": " + item.Value);
        }

        _logger.LogDebug(sb.ToString());
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

        var enforceAttr = context.GetEndpoint()?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();
        return enforceAttr?.Enabled != true;
    }
}

