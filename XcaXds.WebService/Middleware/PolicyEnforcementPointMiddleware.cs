using Abc.Xacml.Context;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Diagnostics;
using System.Net;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;
using XcaXds.WebService.Services;

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
        Stopwatch sw = Stopwatch.StartNew();
        Debug.Assert(!_env.IsProduction() || !_xdsConfig.IgnorePEPForLocalhostRequests, "Warning! 'PEP bypass for local requests' is enabled in production!");

        // If the request is from localhost and environment is development we can ignore PEP.
        var requestIsLocal = httpContext.Connection.RemoteIpAddress != null &&
              (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress) ||
               httpContext.Connection.RemoteIpAddress.ToString() == "::1");

        if (requestIsLocal && _xdsConfig.IgnorePEPForLocalhostRequests == true && _env.IsDevelopment() == true)
        {
            _logger.LogWarning($"{httpContext.TraceIdentifier} - Policy Enforcement Point middleware was bypassed for requests from localhost.");
            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            await _next(httpContext);
            return;
        }


        var endpoint = httpContext.GetEndpoint();
        var enforceAttr = endpoint?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();

        if (enforceAttr == null || enforceAttr.Enabled == false)
        {
            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            await _next(httpContext); // Skip PEP check
            return;
        }

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point middleware was bypassed for requests from localhost.");

        httpContext.Request.EnableBuffering(); // Allows multiple reads

        var sb = new StringBuilder();

        sb.AppendLine($"{httpContext.TraceIdentifier} - Request headers");
        foreach (var item in httpContext.Request.Headers)
        {
            sb.AppendLine(item.Key + ": " + item.Value);
        }

        _logger.LogInformation(sb.ToString());

        var requestBody = await httpContext.Request.GetHttpRequestBodyAsStringAsync();

        _logger.LogDebug($"{httpContext.TraceIdentifier} - Request Body:\n{requestBody}");

        if (httpContext.Request.Headers.ContentType.Any(ct => ct != null && ct.Contains("boundary")))
        {
            requestBody = await HttpRequestResponseExtensions.ReadMultipartContentFromRequest(httpContext);
        }
        requestBody = requestBody.Trim();


        var contentType = httpContext.Request.ContentType?.Split(";").First();
        XacmlContextRequest? xacmlRequest = null;

        bool tokenIsValid = true;

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Request Content-type: {contentType}");

        switch (contentType)
        {
            case Constants.MimeTypes.XopXml:
            case Constants.MimeTypes.MultipartRelated:
            case Constants.MimeTypes.SoapXml:
            case Constants.MimeTypes.Xml:

                var xacmlAction = PolicyRequestMapperSamlService.MapXacmlActionFromSoapAction(PolicyRequestMapperSamlService.GetActionFromSoapEnvelope(requestBody));
                var samlTokenString = PolicyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

                if (string.IsNullOrEmpty(samlTokenString))
                {
                    break;
                }

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

                var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(samlTokenString);
                xacmlRequest = await PolicyRequestMapperSamlService.GetXacmlRequestFromSamlToken(samlToken, xacmlAction, XacmlVersion.Version20);

                break;

            default:
            case Constants.MimeTypes.Json:
                xacmlRequest = await PolicyRequestMapperJsonWebTokenService.GetXacml20RequestFromJsonWebToken(httpContext.Request.Headers);
                break;
        }

        //FIXME maybe some day, do something about JWT aswell?!
        if (xacmlRequest == null && (contentType == Constants.MimeTypes.Json || contentType == Constants.MimeTypes.FhirJson))
        {
        }

        if (xacmlRequest == null && (contentType == Constants.MimeTypes.SoapXml || contentType == Constants.MimeTypes.XopXml || contentType == Constants.MimeTypes.MultipartRelated))
        {
            var soapEnvelope = new SoapEnvelope();

            if (tokenIsValid)
            {
                soapEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "No saml token found in SOAP-message").Value;
            }
            else
            {
                soapEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "Invalid saml token in SOAP-message").Value;
            }

            var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

            await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(soapEnvelope).Content ?? string.Empty);

            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            return;
        }

        var xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(xacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        _logger.LogDebug($"{httpContext.TraceIdentifier} - XACML request:\n{xacmlRequestString}");

        var policySetXml = XacmlSerializer.SerializeXacmlToXml(_debug_policyRepositoryService.GetPoliciesAsXacmlPolicySet(), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var evaluateResponse = _policyDecisionPointService.EvaluateRequest(xacmlRequest);

        _logger.LogInformation($"{httpContext.TraceIdentifier} - Policy Enforcement Point result: {evaluateResponse.Results.FirstOrDefault()?.Decision.ToString()}");

        if (evaluateResponse != null && evaluateResponse.Results.All(res => res.Decision == XacmlContextDecision.Permit))
        {
            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            await _next(httpContext);
        }
        else
        {
            var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

            var soapEnvelopeObject = sxmls.DeserializeSoapMessage<SoapEnvelope>(requestBody);

            var soapEnvelopeResponse = new SoapEnvelope()
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

            sw.Stop();
            _logger.LogInformation($"{httpContext.TraceIdentifier} - Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            return;
        }
    }
}

