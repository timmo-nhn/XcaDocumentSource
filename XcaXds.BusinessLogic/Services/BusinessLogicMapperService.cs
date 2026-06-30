using XcaXds.BusinessLogic.Extensions;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Interfaces;
using XcaXds.Shared;
using XcaXds.Shared.Enums;

namespace XcaXds.BusinessLogic.Services;

public class BusinessLogicMapperService
{
    private readonly NinParserFactory _ninParserFactory;
    public BusinessLogicMapperService(NinParserFactory ninParserFactory)
    {
        _ninParserFactory = ninParserFactory;
    }

    public BusinessLogicParameters MapFromAbacRequestToBusinessLogic(AbacRequest? abacRequest)
    {

        var businessLogic = new BusinessLogicParameters();
        var appliesTo = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Urn.Custom.AppliesTo)?.FirstOrDefault() ?? nameof(AppliesTo.Unknown);
        businessLogic.AppliesTo = Enum.TryParse<AppliesTo>(appliesTo, out var apto) ? apto : AppliesTo.Unknown;
        businessLogic.QueriedSubject = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Urn.Custom.AdhocQueryPatientIdentifier);

        var querySubjectParser = _ninParserFactory.CreateNinParser(businessLogic.QueriedSubject?.Code);
        businessLogic.QueriedSubjectAge = querySubjectParser?.GetAgeFromPatientId(businessLogic.QueriedSubject?.Code) ?? 0;

        businessLogic.Purpose = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.PurposeOfUse) ?? BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.PurposeOfUse_Helsenorge);
        businessLogic.Resource = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.ResourceId20);

        var resourceParser = _ninParserFactory.CreateNinParser(businessLogic.Resource?.Code);
        businessLogic.ResourceAge = resourceParser?.GetAgeFromPatientId(businessLogic.Resource?.Code) ?? 0;

        businessLogic.Subject = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.ProviderIdentifier);
        businessLogic.SubjectOrganization = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.OrganizationId);

        var subjectParser = _ninParserFactory.CreateNinParser(businessLogic.Subject?.Code);
        businessLogic.SubjectAge = subjectParser?.GetAgeFromPatientId(businessLogic.Subject?.Code) ?? 0;

        businessLogic.Scope = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.EhelseScope);
        businessLogic.Role = BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.Role) ?? BusinessLogicExtensions.GetAbacRequestAttributesAsCodedValue(abacRequest, Constants.Saml.Attribute.SubjectRole20);
        businessLogic.Acp = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.XuaAcp)?.FirstOrDefault();
        businessLogic.Bppc = BusinessLogicExtensions.GetAbacRequestAttributeAsString(abacRequest, Constants.Saml.Attribute.BppcDocId)?.FirstOrDefault();

        return businessLogic;
    }
}
