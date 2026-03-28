using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Services.PolicyEnforcementPoint.InputBuilder;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public class SoapSamlXmlPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly PolicyRequestMapperSamlService _policyRequestMapperSamlService;
    private readonly ILogger<SoapSamlXmlPolicyInputStrategy> _logger;

    public SoapSamlXmlPolicyInputStrategy(ILogger<SoapSamlXmlPolicyInputStrategy> logger, PolicyRequestMapperSamlService policyRequestMapperSamlService)
    {
        _logger = logger;
        _policyRequestMapperSamlService = policyRequestMapperSamlService;
    }

    public string[] GetAcceptedContentTypes()
    {
        return
        [
            Constants.MimeTypes.SoapXml,
            Constants.MimeTypes.Xml,
            Constants.MimeTypes.MultipartRelated
        ];
    }

    public async Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig)
    {
        string requestBody;

        requestBody = await HttpRequestResponseExtensions.GetStreamAsStringAsync(context.Request.Body);

        _logger.LogDebug($"{context.TraceIdentifier} - SOAP Envelope body: {requestBody}");

        if (context.Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated)
        {
            requestBody = await MultipartExtensions.ReadMultipartContentFromStream(context.Request.Body, context.Request.ContentType);
        }

        // If BypassPolicyEnforcementPoint is true and ValidateSamlTokenIntegrity is true, we should still skip validating the SAML-token,
        // as SAML-token validation can be seen as a subset of the policy enforcement process
        var shouldBypassTokenValidation = appConfig.ValidateSamlTokenIntegrity;
        if (appConfig.BypassPolicyEnforcementPoint == true)
        {
            shouldBypassTokenValidation = true;
        }

        if (shouldBypassTokenValidation)
        {
            _logger.LogInformation($"{context.TraceIdentifier} - {nameof(appConfig.ValidateSamlTokenIntegrity)} Is true, validating SAML-token");
            var validations = new Saml2SecurityTokenHandler();
            var validator = new Saml2Validator([appConfig.HelseidCert, appConfig.HelsenorgeCert]);

            var samlTokenString = _policyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

            if (string.IsNullOrWhiteSpace(samlTokenString))
            {
                return PolicyInputResult.Fail($"{context.TraceIdentifier} - Fail! No SAML-token in request!");
            }
            var tokenIsValid = validator.ValidateSamlToken(samlTokenString, out var message);

            if (tokenIsValid == false)
            {
                _logger.LogInformation($"{context.TraceIdentifier} - Fail! Invalid SAML-token!\nError: {message}!");
                return PolicyInputResult.Fail($"Invalid SAML-token!\nError: {message}");
            }

            _logger.LogInformation($"{context.TraceIdentifier} - SAML-token is valid");
        }

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(requestBody);

        if (string.IsNullOrEmpty(soapEnvelope.Header.Security?.Assertion?.OuterXml))
        {
            _logger.LogInformation($"{context.TraceIdentifier} - Fail! No SAML-token in request!");
            return PolicyInputResult.Fail($"No SAML-token in request!");
        }

        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelope.Header.Security.Assertion.OuterXml);

        var appliesTo = SamlExtensions.GetIssuerEnumFromSamlTokenIssuer(samlToken?.Assertion.Issuer.Value);

        _logger.LogInformation($"{context.TraceIdentifier} - Issuer: {samlToken?.Assertion.Issuer.Value} Policy AppliesTo: {appliesTo}");

        var xacmlRequest = _policyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, samlToken, appliesTo);

        _logger.LogDebug($"{context.TraceIdentifier} - Generated XACML Request - JSON representation: {JsonSerializer.Serialize(xacmlRequest)}");

        if (xacmlRequest == null)
        {
            return PolicyInputResult.Fail($"Error generating XACML request from SOAP Envelope");
        }

        return PolicyInputResult.Success(xacmlRequest, appliesTo, this);
    }

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);
}
