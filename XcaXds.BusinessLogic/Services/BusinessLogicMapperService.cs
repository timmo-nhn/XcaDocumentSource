using XcaXds.BusinessLogic.Extensions;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.Commons.Interfaces;
using XcaXds.Shared;
using XcaXds.Shared.Enums;

namespace XcaXds.BusinessLogic.Services;

public class BusinessLogicMapperService
{
    private readonly INinParser _ninParser;
    public BusinessLogicMapperService(INinParser ninParser)
    {
        _ninParser = ninParser;
    }

    public BusinessLogicParameters MapFromAbacRequestToBusinessLogic(AbacRequest? abacRequest)
    {
        var businessLogic = new BusinessLogicParameters();
        var appliesTo = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Urn.Custom.AppliesTo)?.FirstOrDefault() ?? nameof(AppliesTo.Unknown);
        businessLogic.AppliesTo = Enum.TryParse<AppliesTo>(appliesTo, out var apto) ? apto : AppliesTo.Unknown;
        businessLogic.QueriedSubject = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Urn.Custom.AdhocQueryPatientIdentifier);
        businessLogic.QueriedSubjectAge = _ninParser.GetAgeFromPatientId(businessLogic.QueriedSubject?.Code);
        businessLogic.Purpose = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.PurposeOfUse) ?? BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.PurposeOfUse_Helsenorge);
        businessLogic.Resource = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.ResourceId20);
        businessLogic.ResourceAge = _ninParser.GetAgeFromPatientId(businessLogic.Resource?.Code);
        businessLogic.Subject = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.ProviderIdentifier);
        businessLogic.SubjectOrganization = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.OrganizationId);
        businessLogic.SubjectAge = _ninParser.GetAgeFromPatientId(businessLogic.Subject?.Code);
        businessLogic.Scope = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.EhelseScope);
        businessLogic.Role = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.Role) ?? BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.SubjectRole20);
        businessLogic.Acp = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.XuaAcp)?.FirstOrDefault();
        businessLogic.Bppc = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.BppcDocId)?.FirstOrDefault();

        return businessLogic;
    }
}
