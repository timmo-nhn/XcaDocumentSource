using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputBuilder;
using XcaXds.WebService.Services;

namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies;

public class SoapSamlXmlPolicyInputStrategy : IPolicyInputStrategy
{
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
        
        if (context.Request.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated)
        {
            requestBody = await MultipartExtensions.ReadMultipartContentFromRequest(context.Request);
        }
        var samlTokenString = PolicyRequestMapperSamlService.GetSamlTokenFromSoapEnvelope(requestBody);

        if (string.IsNullOrEmpty(samlTokenString))
        {
            return PolicyInputResult.Fail($"No SAML-token in request!");
        }

        if (appConfig.ValidateSamlTokenIntegrity)
        {
            var validations = new Saml2SecurityTokenHandler();
            var validator = new Saml2Validator([appConfig.HelseidCert, appConfig.HelsenorgeCert]);

            var tokenIsValid = validator.ValidateSamlToken(samlTokenString, out var message);

            if (tokenIsValid == false)
            {
                return PolicyInputResult.Fail($"Invalid SAML-token!\nError: {message}");
            }
        }

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(requestBody);
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security?.Assertion?.OuterXml);

        var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken?.Assertion.Issuer.Value);

        var xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequest(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, documentRegistry);
        if (xacmlRequest == null)
        {
            return PolicyInputResult.Fail($"Error generating XACML request from SOAP request");
        }

        return PolicyInputResult.Success(xacmlRequest, appliesTo, this);
    }

    public bool CanHandle(string? contentType)
        => GetAcceptedContentTypes().Contains(contentType);
}
