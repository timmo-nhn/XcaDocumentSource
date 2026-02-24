using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.DataManipulators;

/// <summary>
/// Convert requests and responses into formats which are compatible with the existing AtnaLogGeneratorService (SOAP-envelope request and response)
/// ie. convert a FHIR bundle and JWT into soap envelope, which can be handled by AtnaLogGeneratorService
/// </summary>
public class AtnaLogEnricher
{
    public static SoapEnvelope GetMockSoapEnvelopeFromJwt(string? jwtToken, Bundle? fhirBundle, List<RegistryErrorType>? errors, IdentifiableType[] registryObjects)
    {
        if (!string.IsNullOrWhiteSpace(jwtToken) && jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            jwtToken = jwtToken.Substring("Bearer ".Length).Trim();
        }

        XmlElement? samlAssertionElement = GetEnrichedSamlTokenFromTokenAndBundle(jwtToken, fhirBundle, "is_provide_bundle");

        var pnrEnvelope = new SoapEnvelope()
        {
            Header = new()
            {
                MessageId = fhirBundle?.Id,
                Action = Constants.Xds.OperationContract.Iti41Action,
                Security = new Security() { Assertion = samlAssertionElement }
            },
            Body = new()
            {
                RegistryResponse = errors?.Count > 0 ? new() { RegistryErrorList = new() { RegistryError = errors.ToArray() } } : null,
                ProvideAndRegisterDocumentSetRequest = new()
                {
                    SubmitObjectsRequest = new()
                    {
                        RegistryObjectList = registryObjects 
                    }
                },
            }
        };
        pnrEnvelope.Body.RegistryResponse?.EvaluateStatusCode();

        return pnrEnvelope;
    }

    private static XmlElement? GetEnrichedSamlTokenFromTokenAndBundle(string? jwtToken, Bundle? fhirBundle, string requestType)
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

            var samlToken = PolicyRequestMapperJsonWebToken.MapJsonWebTokenToSamlToken(token);
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
                var resourceId = !string.IsNullOrWhiteSpace(patientSystem) ? $"{patientSystem}^{patientValue}" : patientValue;

                samlToken.Assertion.Statements.Add(new Saml2AttributeStatement(new Saml2Attribute(
                    Constants.Saml.Attribute.ResourceId20,
                    resourceId)));

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
