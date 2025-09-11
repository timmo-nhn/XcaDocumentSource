using Abc.Xacml.Context;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Diagnostics;
using System.Net;
using Microsoft.Net.Http.Headers;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.WebService.Attributes;
using Microsoft.AspNetCore.WebUtilities;
using XcaXds.Commons.Models.Hl7.DataType;

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
            _logger.LogWarning("Policy Enforcement Point middleware was bypassed for requests from localhost.");
            await _next(httpContext);
            return;
        }


        var endpoint = httpContext.GetEndpoint();
        var enforceAttr = endpoint?.Metadata.GetMetadata<UsePolicyEnforcementPointAttribute>();

        if (enforceAttr == null || enforceAttr.Enabled == false)
        {
            await _next(httpContext); // Skip PEP check
            return;
        }

        _logger.LogInformation("Running through Policy Enforcement Point middleware...");

        httpContext.Request.EnableBuffering(); // Allows multiple reads

        var requestBody = await httpContext.Request.GetHttpRequestBodyAsStringAsync();

        _logger.LogInformation($"Request Body:\n{requestBody}");

        if (requestBody.StartsWith("--MIMEBoundary") || httpContext.Request.Headers.ContentType.Any(ct => ct != null && ct.Contains("MIMEBoundary")))
        {
            requestBody = await HttpRequestResponseExtensions.ReadMultipartContentFromRequest(httpContext);
        }


        var contentType = httpContext.Request.ContentType?.Split(";").First();
        XacmlContextRequest? xacmlRequest = null;

        _logger.LogInformation($"Request Content-type: {contentType}");

        switch (contentType)
        {
            case Constants.MimeTypes.XopXml:
            case Constants.MimeTypes.SoapXml:

                var xacmlAction = PolicyRequestMapperSamlService.MapXacmlActionFromSoapAction(PolicyRequestMapperSamlService.GetActionFromSoapEnvelope(requestBody));
                var samlTokenString = PolicyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

                if (string.IsNullOrEmpty(samlTokenString))
                {
                    break;
                }

                if (_xdsConfig.ValidateSamlTokenIntegrity)
                {
                    var validations = new TokenValidationParameters()
                    {

                    };
                }

                var samlTokenValidator = new Saml2SecurityTokenHandler();

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

        if (xacmlRequest == null && (contentType == Constants.MimeTypes.SoapXml || contentType == Constants.MimeTypes.XopXml))
        {
            var soapEnvelope = SoapExtensions.CreateSoapFault("Sender", null, "No saml token found in SOAP-message").Value;
            var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
            httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;

            await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(soapEnvelope).Content ?? string.Empty);

            sw.Stop();
            _logger.LogInformation($"Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            return;
        }

        var xacmlRequestString = XacmlSerializer.SerializeXacmlToXml(xacmlRequest, Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var policySetXml = XacmlSerializer.SerializeXacmlToXml(_debug_policyRepositoryService.GetPoliciesAsXacmlPolicySet(), Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var evaluateResponse = _policyDecisionPointService.EvaluateRequest(xacmlRequest);

        _logger.LogInformation($"Policy Enforcement Point result: {evaluateResponse.Results.FirstOrDefault()?.Decision.ToString()}");

        if (evaluateResponse != null && evaluateResponse.Results.All(res => res.Decision == XacmlContextDecision.Permit))
        {
            sw.Stop();
            _logger.LogInformation($"Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
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

            _logger.LogInformation($"Policy Enforcement Point has denied the request: id {soapEnvelopeObject.Header.MessageId}");

            var registryResponse = new RegistryResponseType();
            registryResponse.AddError(XdsErrorCodes.XDSRegistryError, $"Access denied", _xdsConfig.HomeCommunityId);

            soapEnvelopeResponse.Body ??= new();
            httpContext.Response.ContentType = Constants.MimeTypes.SoapXml;
            
            SoapExtensions.PutRegistryResponseInTheCorrectPlaceAccordingToSoapAction(soapEnvelopeResponse, registryResponse);


            await httpContext.Response.WriteAsync(sxmls.SerializeSoapMessageToXmlString(soapEnvelopeResponse).Content ?? string.Empty);

            sw.Stop();
            _logger.LogInformation($"Ran through PolicyEnforcementPoint-middleware in {sw.ElapsedMilliseconds} ms");
            return;
        }
    }
}

