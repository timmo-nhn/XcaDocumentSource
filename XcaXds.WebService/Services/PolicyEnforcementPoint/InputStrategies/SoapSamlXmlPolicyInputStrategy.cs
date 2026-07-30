using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces.PolicyEnforcementPoint.InputStrategies;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint.InputBuilder;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.WebService.Services.PolicyEnforcementPoint.Policy.RequestMappers;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.InputStrategies;

public class SoapSamlXmlPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly SamlPolicyRequestMapper _policyRequestMapperSamlService;
    private readonly ILogger<SoapSamlXmlPolicyInputStrategy> _logger;
    private readonly SamlValidatorService _samlValidator;
    public SoapSamlXmlPolicyInputStrategy(ILogger<SoapSamlXmlPolicyInputStrategy> logger, SamlPolicyRequestMapper policyRequestMapperSamlService, SamlValidatorService samlValidator)
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

        _logger.LogDebug("{traceIdentifier} - SOAP Envelope body: {requestBody}", context.TraceIdentifier, requestBody);

        if (context.Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated)
        {
            requestBody = await MultipartExtensions.ReadMultipartContentFromStream(context.Request.Body, context.Request.ContentType);
        }

        // If BypassPolicyEnforcementPoint is true and ValidateSamlTokenIntegrity is true, we should still skip validating the SAML-token,
        // as SAML-token validation can be seen as a subset of the policy enforcement process
        var shouldBypassTokenValidation = !appConfig.SamlValidateSamlTokenIntegrity;

		if (appConfig.CanOverrideValidateSamlTokenIntegrityWithQueryParameter)
        {
			// This functionality is only used by PJD REST API and Pasientens journaldokumenter Test EPJ to override the appConfig.ValidateSamlTokenIntegrity value for testing purposes
			var query = context.Request.Query;
            if (query != null)
            {
				// read query parameter "validateSamlTokenIntegrity" and override the appConfig.ValidateSamlTokenIntegrity value if it exists
				if (query.ContainsKey("validateSamlTokenIntegrity"))
				{
					var validateSamlTokenIntegrityQueryParam = query["validateSamlTokenIntegrity"].ToString();
					if (bool.TryParse(validateSamlTokenIntegrityQueryParam, out var validateSamlTokenIntegrity))
					{
						shouldBypassTokenValidation = !validateSamlTokenIntegrity;
					}
				}
			}
		}
		
		if (appConfig.BypassPolicyEnforcementPoint == true)
        {
            shouldBypassTokenValidation = true;
        }

        if (!shouldBypassTokenValidation)
        {
            _logger.LogInformation("{traceIdentifier} - {samlValidateSamlTokenIntegrity} Is true, validating SAML-token", context.TraceIdentifier, nameof(appConfig.SamlValidateSamlTokenIntegrity));
            var validations = new Saml2SecurityTokenHandler();

            var samlTokenString = _policyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

            if (string.IsNullOrWhiteSpace(samlTokenString))
            {
                return PolicyInputResult.Fail($"{context.TraceIdentifier} - Fail! No SAML-token in request!");
            }

            var validator = await _samlValidator.CreateSamlValidator();
            var validationMessage = validator.ValidateSamlToken(samlTokenString, out var success);

            if (success == false)
            {
                _logger.LogInformation("{traceIdentifier} - Fail! Invalid SAML-token!\nError: {validationMessage}!", context.TraceIdentifier, validationMessage);
                return PolicyInputResult.Fail($"Invalid SAML-token!\nError: {validationMessage}");
            }

            _logger.LogInformation("{traceIdentifier} - SAML-token is valid", context.TraceIdentifier);
        }

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(requestBody);

        if (string.IsNullOrEmpty(soapEnvelope.Header.Security?.Assertion?.OuterXml))
        {
            _logger.LogInformation("{traceIdentifier} - Fail! No SAML-token in request!", context.TraceIdentifier);
            return PolicyInputResult.Fail($"No SAML-token in request!");
        }
        
        var abacRequest = _policyRequestMapperSamlService.MapToAbacRequest(soapEnvelope);

        _logger.LogDebug("{traceIdentifier} - Generated ABAC Request - JSON representation: {abacRequest}", context.TraceIdentifier, JsonSerializer.Serialize(abacRequest));

        if (abacRequest == null)
        {
            return PolicyInputResult.Fail($"Error generating ABAC request from SOAP Envelope");
        }

        return PolicyInputResult.Success(abacRequest, this);
    }
}
