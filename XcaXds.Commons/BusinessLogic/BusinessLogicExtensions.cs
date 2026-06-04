using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.DataManipulators.BusinessLogic;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs. Allows for partial (non-atomic) filtering of the document list
/// </summary>
public static class BusinessLogicExtensions
{
    public static int GetAgeFromPatientId(string? patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId) || patientId.Length != 11) return 0;

        var patientNin = Hl7FhirExtensions.ParseNorwegianNinToDateTime(patientId);

        var year = DateTime.Today.Year - (patientNin.HasValue ? patientNin.Value.Year : 0);

        return year;
    }

    public static BusinessLogicParameters MapFromAbacRequestToBusinessLogic(AbacRequest? abacRequest)
    {
        var businessLogic = new BusinessLogicParameters();
        var appliesTo = GetAbacRequestAttributeAsString(abacRequest, Constants.Urn.Custom.AppliesTo)?.FirstOrDefault() ?? nameof(AppliesTo.Unknown);
        businessLogic.AppliesTo = Enum.TryParse<AppliesTo>(appliesTo, out var apto) ? apto : AppliesTo.Unknown;
        businessLogic.QueriedSubject = GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Urn.Custom.AdhocQueryPatientIdentifier);
        businessLogic.QueriedSubjectAge = GetAgeFromPatientId(businessLogic.QueriedSubject?.Code);
        businessLogic.Purpose = GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.PurposeOfUse) ?? GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.PurposeOfUse_Helsenorge);
        businessLogic.Resource = GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.ResourceId20);
        businessLogic.ResourceAge = GetAgeFromPatientId(businessLogic.Resource?.Code);
        businessLogic.Subject = GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.ProviderIdentifier);
        businessLogic.SubjectOrganization = GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.OrganizationId);
        businessLogic.SubjectAge = GetAgeFromPatientId(businessLogic.Subject?.Code);
        businessLogic.Scope = GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.EhelseScope);
        businessLogic.Role = GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.Role) ?? GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.SubjectRole20);
        businessLogic.Acp = GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.XuaAcp)?.FirstOrDefault();
        businessLogic.Bppc = GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.BppcDocId)?.FirstOrDefault();

        return businessLogic;
    }

    private static string[]? GetAbacRequestAttributeAsString(AbacRequest? abacRequest, string attribute)
    {
        return abacRequest?.Attributes?.Where(att => att.Key.Contains(attribute)).SelectMany(v => v.Value).ToArray();
    }

    private static CodedValue? GetAbacRequestAttributesAsCodedValue(AbacRequest? abacRequest, string attribute)
    {
        if (abacRequest?.Attributes == null)
            return null;

        var indexed = abacRequest.Attributes.IndexAttributesWithPrefix(attribute);

        if (!indexed.TryGetValue($"{attribute}:code", out var code))
            return null;

        return new CodedValue
        {
            Code = code,
            CodeSystem = indexed.GetValueOrDefault($"{attribute}:codeSystem"),
            DisplayName = indexed.GetValueOrDefault($"{attribute}:displayName")
        };
    }
}