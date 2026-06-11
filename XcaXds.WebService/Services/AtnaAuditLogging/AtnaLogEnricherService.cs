using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Extensions;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;

namespace XcaXds.WebService.Services.AtnaAuditLogging;

/// <summary>
/// Convert requests and responses into formats which are compatible with the existing AtnaLogGeneratorService (SOAP-envelope request and response)
/// ie. convert a FHIR bundle and JWT into soap envelope, which can be handled by AtnaLogGeneratorService
/// </summary>
public class AtnaLogEnricherService
{
    private readonly ILogger<AtnaLogEnricherService> _logger;
    private readonly PolicyRequestMapperJsonWebTokenService _policyRequestMapperJwtService;
    private readonly JwtToSamlTransformerService _jwtToSamlTransformerService;
    private readonly TerminologyService _terminologyService;

    public AtnaLogEnricherService(
        ILogger<AtnaLogEnricherService> logger,
        PolicyRequestMapperJsonWebTokenService policyRequestMapperJwtService,
        JwtToSamlTransformerService jwtToSamlTransformerService,
        TerminologyService terminologyService)
    {
        _logger = logger;
        _policyRequestMapperJwtService = policyRequestMapperJwtService;
        _jwtToSamlTransformerService = jwtToSamlTransformerService;
        _terminologyService = terminologyService;
    }

    public SoapEnvelope GetMockSoapEnvelopeFromJwtAndBundle(AdditionalParameters additionalParameters, string? jwtToken, Bundle? fhirBundle, IdentifiableType[]? registryObjects)
    {
        if (!string.IsNullOrWhiteSpace(jwtToken) && jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            jwtToken = jwtToken.Substring("Bearer ".Length).Trim();
        }


        XmlElement? samlAssertionElement = GetEnrichedSamlTokenFromTokenAndBundle(jwtToken, fhirBundle);

        var errors = fhirBundle?.Entry
            .Select(res => res.Resource)
            .OfType<OperationOutcome>()
            .FirstOrDefault();

        var xdsErrors = XdsErrorToOperationOutcomeMapper.GetXdsErrorsFromOperationOutcome(errors);

        var pnrEnvelope = new SoapEnvelope()
        {
            Header = new()
            {
                MessageId = fhirBundle?.Id,
                Action = "ITI-65",
                Security = new Security() { Assertion = samlAssertionElement }
            },
            Body = new()
            {
                RegistryResponse = xdsErrors?.RegistryError.Length > 0 ? new() { RegistryErrorList = xdsErrors } : null,
            }
        };

        switch (additionalParameters.HttpMethod)
        {
            case "POST":
                if (registryObjects?.Length > 0)
                {
                    pnrEnvelope.Body.ProvideAndRegisterDocumentSetRequest = new()
                    {
                        SubmitObjectsRequest = new()
                        {
                            RegistryObjectList = registryObjects!
                        }
                    };
                }
                break;

            default:
                if (registryObjects?.Length > 0)
                {
                    pnrEnvelope.Body.AdhocQueryResponse = new() { RegistryObjectList = registryObjects! };
                }
                break;
        }

        pnrEnvelope.Body.RegistryResponse?.EvaluateStatusCode();

        return pnrEnvelope;
    }

    private XmlElement? GetEnrichedSamlTokenFromTokenAndBundle(string? jwtToken, Bundle? fhirBundle)
    {
        var patient = fhirBundle?.Entry
            .Where(e => e.Resource is Patient)
            .Select(e => (Patient?)e.Resource)
            .FirstOrDefault();

        var handler = new JwtSecurityTokenHandler();

        XmlElement? samlAssertionElement = null;

        if (handler.CanReadToken(jwtToken) == true)
        {

            var token = handler.ReadJwtToken(jwtToken);

            var samlToken = _jwtToSamlTransformerService.MapJsonWebTokenToSamlToken(token);

            // Enrich SAML assertion with patient context from the submitted Bundle (if present).
            // This is used by downstream auditing/policy components expecting patient/resource attributes in the token.
            var samlPatientIdentifier = patient?.Identifier?.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i?.Value))
                ?? patient?.Identifier?.FirstOrDefault();

            if (samlPatientIdentifier != null && !string.IsNullOrWhiteSpace(samlPatientIdentifier.Value))
            {
                var patientSystem = samlPatientIdentifier.System?.NoUrn();
                var patientValue = samlPatientIdentifier.Value;

                var resourceId = new CX(patientValue, patientSystem);

                samlToken.Assertion.Statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    //Constants.Saml.Attribute.ResourceId20
                    _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Authentication.SamlAttributes, "ResourceId20")?.Values.FirstOrDefault(),
                    resourceId.Serialize())));

                var patientName = patient?.Name?.FirstOrDefault();
                var patientGiven = patientName?.Given?.FirstOrDefault(g => !string.IsNullOrWhiteSpace(g));
                var patientFamily = patientName?.Family;

                if (!string.IsNullOrWhiteSpace(patientGiven))
                {
                    samlToken.Assertion.Statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                        "patient_given",
                        patientGiven)));
                }

                if (!string.IsNullOrWhiteSpace(patientFamily))
                {
                    samlToken.Assertion.Statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                        "patient_family",
                        patientFamily)));
                }
            }

            // Ensure the SAML token is serializable: SAML2 AttributeStatement must contain at least one Attribute.
            var emptyAttributeStatements = samlToken.Assertion.Statements
                .OfType<Saml2AttributeStatement>()
                .Where(s => s.Attributes == null || s.Attributes.Count == 0)
                .Cast<Saml2Statement>()
                .ToList();

            foreach (var statement in emptyAttributeStatements)
            {
                samlToken.Assertion.Statements.Remove(statement);
            }

            var samlHandler = new Saml2SecurityTokenHandler();
            var samlXml = samlHandler.WriteToken(samlToken);
            var samlDoc = new XmlDocument() { PreserveWhitespace = true };
            samlDoc.LoadXml(samlXml);
            samlAssertionElement = samlDoc.DocumentElement;
        }

        return samlAssertionElement;
    }
}
