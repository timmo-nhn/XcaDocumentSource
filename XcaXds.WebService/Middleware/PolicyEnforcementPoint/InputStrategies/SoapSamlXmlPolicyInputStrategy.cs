using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

public class SoapSamlXmlPolicyInputStrategy : IPolicyInputStrategy
{
    private readonly ILogger<SoapSamlXmlPolicyInputStrategy> _logger;

    public SoapSamlXmlPolicyInputStrategy(ILogger<SoapSamlXmlPolicyInputStrategy>logger)
    {
        _logger = logger;
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

    public async Task<PolicyInputResult> BuildAsync(HttpContext context, ApplicationConfig appConfig, IEnumerable<RegistryObjectDto> documentRegistry)
    {
        string requestBody;

        requestBody = await HttpRequestResponseExtensions.GetHttpRequestBodyAsStringAsync(context.Request);
        
        _logger.LogDebug($"{context.TraceIdentifier} - SOAP Envelope body: {requestBody}");

        if (context.Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated)
        {
            requestBody = await MultipartExtensions.ReadMultipartContentFromRequest(context.Request);
        }

        if (appConfig.ValidateSamlTokenIntegrity)
        {
            _logger.LogInformation($"{context.TraceIdentifier} - {nameof(appConfig.ValidateSamlTokenIntegrity)} Is true, validating SAML-token");
            var validations = new Saml2SecurityTokenHandler();
            var validator = new Saml2Validator([appConfig.HelseidCert, appConfig.HelsenorgeCert]);

            var samlTokenString = PolicyRequestMapperSaml.GetSamlTokenFromSoapEnvelope(requestBody);
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
        
        var samlToken = PolicyRequestMapperSaml.ReadSamlToken(soapEnvelope.Header.Security.Assertion.OuterXml);
        
        var appliesTo = PolicyRequestMapperSaml.GetIssuerEnumFromSamlTokenIssuer(samlToken?.Assertion.Issuer.Value);

        _logger.LogInformation($"{context.TraceIdentifier} - Issuer: {samlToken?.Assertion.Issuer.Value} Policy AppliesTo: {appliesTo}");

        var xacmlRequest = PolicyRequestMapperSaml.GetXacmlRequest(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, documentRegistry);
        
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
