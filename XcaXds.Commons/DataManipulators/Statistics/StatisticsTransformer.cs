using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Cryptography;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.Commons.Models.Soap;

namespace XcaXds.Commons.DataManipulators.Statistics;

public static class StatisticsTransformer
{
    public static UserAccessEntry TransformToUserAccessEntry(SoapEnvelopeAndFields inputFields)
    {
        var soapEnvelope = inputFields.SoapEnvelope;

        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelope.Header?.Security?.Assertion?.OuterXml);

        var statements = samlToken?.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var subjectIdStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name.IsAnyOf(Constants.Saml.Attribute.ProviderIdentifier))?.Values.FirstOrDefault());
        var subjectIdValue = (subjectIdStatement?.Code + "^" + subjectIdStatement?.CodeSystem).Trim('^');

        var userAccessEntry = new UserAccessEntry
        {
            SubjectIdHash = GetSamlAttributeAsHashedString(statements, Constants.Saml.Attribute.ProviderIdentifier),
            ResourceIdHash = GetSamlAttributeAsHashedString(statements, Constants.Saml.Attribute.ResourceId10, Constants.Saml.Attribute.ResourceId20),

            SubjectOrganization = GetSamlAttributeAsCodedValue(statements, Constants.Saml.Attribute.OrganizationId),
            SubjectOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.Organization),

            SubjectChildOrganization = GetSamlAttributeAsCodedValue(statements, Constants.Saml.Attribute.ChildOrganization),
            SubjectChildOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.TrustChildOrgName),
            AccessBasis = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.XuaAcp),

            DocumentSourceOid = GetDocumentSourceOidFromSoapEnvelope(soapEnvelope),

            Endpoint = inputFields.Path,
            Action = soapEnvelope.Header?.Action,
            ResponseStatusCode = inputFields.StatusCode,
            AccessTime = inputFields.AccessTime,
            ElapsedTimeMillis = inputFields.ElapsedMilliseconds,
        };

        return userAccessEntry;
    }

    private static string? GetDocumentSourceOidFromSoapEnvelope(SoapEnvelope soapEnvelope)
    {
        return soapEnvelope.Body.RetrieveDocumentSetRequest?.DocumentRequest?.FirstOrDefault()?.HomeCommunityId;
    }

    private static string? GetSamlAttributeAsString(List<Saml2Attribute>? statements, params string[] attributeNames)
    {
        return GetSamlAttributeAsCodedValue(statements, attributeNames)?.Code ?? Constants.Oid.Saml.Acp.NullValue;
    }

    private static CodedValue? GetSamlAttributeAsCodedValue(List<Saml2Attribute>? statements, params string[] attributeNames)
    {
        var subjectOrganization = statements?.FirstOrDefault(s => s.Name.IsAnyOf(attributeNames))?.Values.FirstOrDefault();
        return SamlExtensions.GetSamlAttributeValueAsCodedValue(subjectOrganization);
    }

    private static string? GetSamlAttributeAsHashedString(List<Saml2Attribute>? statements, params string[] attributeNames)
    {
        var samlAttributeCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name.IsAnyOf(attributeNames))?.Values.FirstOrDefault());
        var samlStatement = (samlAttributeCoded?.Code + "^" + samlAttributeCoded?.CodeSystem).Trim('^');

        return string.IsNullOrWhiteSpace(samlStatement) ? "Unknown" : Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(samlStatement)));
    }
}