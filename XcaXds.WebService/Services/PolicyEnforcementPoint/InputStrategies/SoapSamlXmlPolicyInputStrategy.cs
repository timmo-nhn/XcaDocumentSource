using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint.InputBuilder;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.WebService.Services.Policy;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public class SoapSamlXmlPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly PolicyRequestMapperSamlService _policyRequestMapperSamlService;
    private readonly ILogger<SoapSamlXmlPolicyInputStrategy> _logger;
    private readonly Saml2Validator _samlValidator;
    public SoapSamlXmlPolicyInputStrategy(ILogger<SoapSamlXmlPolicyInputStrategy> logger, PolicyRequestMapperSamlService policyRequestMapperSamlService, Saml2Validator samlValidator)
    {
        _logger = logger;
        _policyRequestMapperSamlService = policyRequestMapperSamlService;
        _samlValidator = samlValidator;
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

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);

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
        var shouldBypassTokenValidation = !appConfig.ValidateSamlTokenIntegrity;
        if (appConfig.BypassPolicyEnforcementPoint == true)
        {
            shouldBypassTokenValidation = true;
        }

        if (!shouldBypassTokenValidation)
        {
            _logger.LogInformation($"{context.TraceIdentifier} - {nameof(appConfig.ValidateSamlTokenIntegrity)} Is true, validating SAML-token");
            var validations = new Saml2SecurityTokenHandler();

            var samlTokenString = _policyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

            if (string.IsNullOrWhiteSpace(samlTokenString))
            {
                return PolicyInputResult.Fail($"{context.TraceIdentifier} - Fail! No SAML-token in request!");
            }

            await _samlValidator.InitValidatorIfNotInited();
            var tokenIsValid = _samlValidator.ValidateSamlToken(samlTokenString, out var message);

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
        
        var abacRequest = _policyRequestMapperSamlService.GetAbacRequestFromSoapEnvelope(soapEnvelope);

        _logger.LogDebug($"{context.TraceIdentifier} - Generated ABAC Request - JSON representation: {JsonSerializer.Serialize(abacRequest)}");

        if (abacRequest == null)
        {
            return PolicyInputResult.Fail($"Error generating ABAC request from SOAP Envelope");
        }

        return PolicyInputResult.Success(abacRequest, this);
    }
}
