using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Source.Source;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services;
using Task = System.Threading.Tasks.Task;


namespace XcaXds.WebService.Middleware;

public class PolicyEnforcementPointMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyEnforcementPointMiddleware> _logger;
    private readonly ApplicationConfig _xdsConfig;
    private readonly IWebHostEnvironment _env;
    private readonly PolicyDecisionPointService _policyDecisionPointService;
    private readonly MonitoringStatusService _monitoringService;
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly RegistryWrapper _registryWrapper;

    public PolicyEnforcementPointMiddleware(
        RequestDelegate next,
        ILogger<PolicyEnforcementPointMiddleware> logger,
        ApplicationConfig xdsConfig,
        IWebHostEnvironment env,
        PolicyDecisionPointService policyDecisionPointService,
        MonitoringStatusService monitoringService,
        PolicyRepositoryService policyRepositoryService,
        RegistryWrapper registryWrapper
        )
    {
        _next = next;
        _logger = logger;
        _xdsConfig = xdsConfig;
        _env = env;
        _policyDecisionPointService = policyDecisionPointService;
        _monitoringService = monitoringService;
        _policyRepositoryService = policyRepositoryService;
        _registryWrapper = registryWrapper;
    }


    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (_xdsConfig.BypassPolicyEnforcementPoint)
        {
            _logger.LogWarning("BypassPolicyEnforcementPoint is true! Policy Enforcement Point was bypassed.");
            await _next(httpContext);
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();
        Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "Warning! 'PEP bypass for local requests' is enabled in production!");

        // If the request is from localhost and environment is development we can ignore PEP.
        var requestIsLocal = httpContext.Connection.RemoteIpAddress != null &&
              (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress) ||
               httpContext.Connection.RemoteIpAddress.ToString() == "::1");

        if (requestIsLocal && _xdsConfig.IgnorePEPForLocalhostRequests == true && _env.IsDevelopment() == true)
        {
            sw.Stop();
            _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point middleware was bypassed for requests from localhost.");
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Bypassed PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            await _next(httpContext);
            return;
        }

        var endpoint = httpContext.GetEndpoint();
        var enforceAttr = endpoint?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();

        if (enforceAttr == null || enforceAttr.Enabled == false)
        {
            sw.Stop();
            _logger.LogInformation($"Request endpoint doesnt use PEP");
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            await _next(httpContext); // Skip PEP check
            return;
        }

        httpContext.Request.EnableBuffering(); // Allows multiple reads

        var sb = new StringBuilder();

        sb.AppendLine($"{httpContext.TraceIdentifier} - Request headers");
        foreach (var item in httpContext.Request.Headers)
        {
            sb.AppendLine(item.Key + ": " + item.Value);
        }

        _logger.LogDebug(sb.ToString());

        var requestBody = await httpContext.Request.GetHttpRequestBodyAsStringAsync();

        _logger.LogDebug($"{httpContext.TraceIdentifier} - Request Body:\n{requestBody}");


        if (httpContext.Request.Headers.ContentType.Any(ct => ct != null && ct.Contains("boundary")))
        {
            requestBody = await HttpRequestResponseExtensions.ReadMultipartContentFromRequest(httpContext);
        }

        requestBody = requestBody.Trim();

        var contentType = httpContext.Request.ContentType?.Split(";").First();
        _logger.LogInformation($"{httpContext.TraceIdentifier} - Request Content-type: {contentType}");

        
        bool tokenIsValid = true;
        var xacmlRequestAppliesTo = Issuer.Unknown;
        XacmlContextRequest? xacmlRequest = null;
        RestfulApiResponse? restfulApiResponse = null;

        switch (contentType)
        {
            case Constants.MimeTypes.Json:
            case Constants.MimeTypes.FhirJson:
                var fhirJsonParser = new FhirJsonParser();
                var fhirBundle = fhirJsonParser.Parse<Bundle>(requestBody);

                var jwtToken = httpContext.Request.Headers["Authorization"].FirstOrDefault();

                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(jwtToken) == false)
                {
                    restfulApiResponse ??= new RestfulApiResponse(false, "Invalid or missing JWT");
                    goto default;
                }
                var requestUrlPath = httpContext.Request.Path;

                var token = handler.ReadJwtToken(jwtToken);
                xacmlRequest = PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(token, fhirBundle, httpContext.Request.Path, httpContext.Request.Method);

                break;

            case Constants.MimeTypes.XopXml:
            case Constants.MimeTypes.MultipartRelated:
            case Constants.MimeTypes.SoapXml:
            case Constants.MimeTypes.Xml:

                var samlTokenString = PolicyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

                if (!string.IsNullOrEmpty(samlTokenString))
                {
                    if (_xdsConfig.ValidateSamlTokenIntegrity)
                    {
                        var validations = new Saml2SecurityTokenHandler();
                        var validator = new Saml2Validator([_xdsConfig.HelseidCert, _xdsConfig.HelsenorgeCert]);

                        tokenIsValid = validator.ValidateSamlToken(samlTokenString, out var message);

                        if (tokenIsValid == false)
                        {
                            _logger.LogInformation($"{httpContext.TraceIdentifier} - Invalid SAML-token!");
                            _logger.LogInformation($"{httpContext.TraceIdentifier} - {message}");
                            break;
                        }
                    }

                    _logger.LogInformation($"{httpContext.TraceIdentifier} - Saml token is valid!");

                    var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(requestBody);
                    var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);

                    var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Assertion.Issuer.Value);

                    var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();
                    xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, documentRegistry);

                    xacmlRequestAppliesTo = appliesTo;
                }

                if (xacmlRequest == null)
                {
                    var soapResponseEnvelope = new SoapEnvelope();

                    if (tokenIsValid)
                    {
                        soapResponseEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "No saml token found in SOAP-message").Value;
                    }
                    else
                    {
                        soapResponseEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "Invalid saml token in SOAP-message").Value;
                    }

                    var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
                    httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

                    await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(soapResponseEnvelope).Content ?? string.Empty);

                    sw.Stop();
                    _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:tokeninvalid", sw.ElapsedMilliseconds);
                    _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
                    return;
                }
                break;

            default:

                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                httpContext.Response.ContentType = Constants.MimeTypes.Json;

                restfulApiResponse ??= new RestfulApiResponse(false, $"Unknown or unspecified content type: {httpContext.Request.ContentType}".TrimEnd([':', ' ']));

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(restfulApiResponse, Constants.JsonDefaultOptions.DefaultSettings));
                sw.Stop();
                _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:deny", sw.ElapsedMilliseconds);
                _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
                return;
        }


        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            var xacmlPolicySet = XacmlSerializer.SerializeXacmlToXml(_policyRepositoryService.GetPoliciesAsXacmlPolicySet(xacmlRequestAppliesTo), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            var xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(xacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            _logger.LogDebug($"{httpContext.TraceIdentifier} - XACML request:\n{xacmlRequestString}");
        }

        var evaluateResponse = _policyDecisionPointService.EvaluateXacmlRequest(xacmlRequest, xacmlRequestAppliesTo);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point result: {evaluateResponse.Results.FirstOrDefault()?.Decision.ToString()}");

        if (evaluateResponse != null && evaluateResponse.Results.All(res => res.Decision == XacmlContextDecision.Permit))
        {
            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:permit", sw.ElapsedMilliseconds);
            await _next(httpContext);
        }
        else
        {
            SoapEnvelope? soapEnvelopeResponse = null!; 

			if (contentType == Constants.MimeTypes.XopXml
				|| contentType == Constants.MimeTypes.MultipartRelated
				|| contentType == Constants.MimeTypes.SoapXml
				|| contentType == Constants.MimeTypes.Xml)
            {
                var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

                var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(requestBody);

                soapEnvelopeResponse = new SoapEnvelope()
                {
                    Header = new()
                    {
                        Action = soapEnvelopeObject.GetCorrespondingResponseAction(),
                        MessageId = Guid.NewGuid().ToString(),
                        RelatesTo = soapEnvelopeObject.Header.MessageId,
                    }
                };

                _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point has denied the request: id {soapEnvelopeObject.Header.MessageId}");

				var registryResponse = new RegistryResponseType();
				registryResponse.AddError(XdsErrorCodes.XDSRegistryError, $"Access denied", _xdsConfig.HomeCommunityId);

				soapEnvelopeResponse.Body ??= new();
				httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

				SoapExtensions.PutRegistryResponseInTheCorrectPlaceAccordingToSoapAction(soapEnvelopeResponse, registryResponse);

				await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(soapEnvelopeResponse).Content ?? string.Empty);
			}
            else if (contentType == Constants.MimeTypes.Json || contentType == Constants.MimeTypes.FhirJson)
            {
                restfulApiResponse ??= new RestfulApiResponse(false, "Access denied");
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                httpContext.Response.ContentType = Constants.MimeTypes.Json;
                _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point has denied the request.");
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(restfulApiResponse, Constants.JsonDefaultOptions.DefaultSettings));
            }
            else
            {

            }
			
            sw.Stop();
            _monitoringService.ResponseTimes.Add("urn:no:nhn:xcads:pep:deny", sw.ElapsedMilliseconds);
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            return;
        }
    }
}

