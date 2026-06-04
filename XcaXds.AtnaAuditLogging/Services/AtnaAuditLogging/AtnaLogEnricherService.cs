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

namespace XcaXds.AtnaAuditLogging.Services.AtnaAuditLogging;

/// <summary>
/// Convert requests and responses into formats which are compatible with the existing AtnaLogGeneratorService (SOAP-envelope request and response)
/// ie. convert a FHIR bundle and JWT into soap envelope, which can be handled by AtnaLogGeneratorService
/// </summary>
public class AtnaLogEnricherService
{
    private readonly ILogger<AtnaLogEnricherService> _logger;
    private readonly PolicyRequestMapperJsonWebTokenService _policyRequestMapperJwtService;

    public AtnaLogEnricherService(ILogger<AtnaLogEnricherService> logger, PolicyRequestMapperJsonWebTokenService policyRequestMapperJwtService)
    {
        _logger = logger;
        _policyRequestMapperJwtService = policyRequestMapperJwtService;
    }

    public SoapEnvelope GetMockSoapEnvelopeFromJwtAndBundle(AdditionalParameters additionalParameters, string? jwtToken, Bundle? fhirBundle, IdentifiableType?[]? registryObjects)
    {
        if (!string.IsNullOrWhiteSpace(jwtToken) && jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            jwtToken = jwtToken.Substring("Bearer ".Length).Trim();
        }

        // HAYO! RequestTypeInSamlToken - This is maybe a bit "jank", but in SOAP-context this is an OK and pragmatic way to transport arbitrary stuff
        var requestType = (additionalParameters.HttpMethod, additionalParameters.UrlPath) switch
        {
            ("POST", var path) when path != null && path.StartsWith("/R4/fhir") && path.EndsWith("/$validate")
                => "is_validate_resource",

            ("POST", _)
                => "is_provide_bundle",

            ("DELETE", _)
                => "is_delete_bundle",

            _ 
                => "is_query_bundle"
        };

        XmlElement? samlAssertionElement = GetEnrichedSamlTokenFromTokenAndBundle(jwtToken, fhirBundle, requestType);

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

            case "DELETE":
                if (registryObjects?.Length > 0)
                {
                    pnrEnvelope.Body.RemoveObjectsRequest = new()
                    {
                        ObjectRefList = new()
                        {
                            ObjectRef = [.. registryObjects.Select(obj => new ObjectRefType() { Id = obj?.Id })]
                        }
                    };
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

    private XmlElement? GetEnrichedSamlTokenFromTokenAndBundle(string? jwtToken, Bundle? fhirBundle, string requestType)
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

            var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(token);
            samlToken.Assertion.Statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                        requestType,
                        "true")));

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
                    Constants.Saml.Attribute.ResourceId20,
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
