using Abc.Xacml.Context;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class PolicyDecisionPointService
{
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyDecisionPointService(PolicyRepositoryWrapper policyRepositoryWrapper)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
    }

    public XacmlContextResponse EvaluateSoapRequest(SoapEnvelope soapEnvelope, Saml2SecurityToken samlToken, Issuer requestAppliesTo)
    {
        if (requestAppliesTo == Issuer.Helsenorge)
        {
            var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

            var resourceId = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.ResourceId10 || stmnt.Name == Constants.Saml.Attribute.ResourceId20)?.Values.FirstOrDefault();
            var providerId = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values.FirstOrDefault();
            var acp = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.XuaAcp)?.Values.FirstOrDefault() ?? Constants.Oid.Saml.AcpNullValue;
            var bppc = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.BppcDocId)?.Values.FirstOrDefault() ?? Constants.Oid.Saml.BppcNullValue;

            var resourceValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(resourceId);
            var providerValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(providerId);
        }

        return new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.Permit));
    }

    public XacmlContextResponse EvaluateXacmlRequest(XacmlContextRequest? xacmlRequest, Issuer xacmlRequestAppliesTo)
    {
        return _policyRepositoryWrapper.EvaluateRequest_V20(xacmlRequest, xacmlRequestAppliesTo);
    }
}
