using System.Text.Json;
using Abc.Xacml.Context;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class PolicyDecisionPointService
{
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;
    private readonly RegistryWrapper _registryWrapper;

    public PolicyDecisionPointService(PolicyRepositoryWrapper policyRepositoryWrapper, RegistryWrapper registryWrapper)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
        _registryWrapper = registryWrapper;
    }

    public XacmlContextResponse EvaluateSoapRequest(SoapEnvelope soapEnvelope, Saml2SecurityToken samlToken, Issuer requestAppliesTo, List<RegistryObjectDto> documentRegistry)
    {
        var result = XacmlContextDecision.NotApplicable;
        var contextResult = new XacmlContextResult(result);
        var soapAction = soapEnvelope.Header.Action;

        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        // Saml values
        var resourceId = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.ResourceId10 || stmnt.Name == Constants.Saml.Attribute.ResourceId20)?.Values.FirstOrDefault();
        var providerId = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values.FirstOrDefault();
        var acp = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.XuaAcp)?.Values.FirstOrDefault() ?? Constants.Oid.Saml.Acp.NullValue;
        var bppc = statements.FirstOrDefault(stmnt => stmnt.Name == Constants.Saml.Attribute.BppcDocId)?.Values.FirstOrDefault() ?? Constants.Oid.Saml.Bppc.NullValue;


        var resourceValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(resourceId);
        var resourceValueJson = JsonSerializer.Serialize(resourceValue);

        var providerValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(providerId);
        var providerValueJson = JsonSerializer.Serialize(providerValue);

        // Request values
        var retrieveDocumentSet = soapEnvelope.Body.RetrieveDocumentSetRequest?.DocumentRequest;
        var registry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var documentEntries = retrieveDocumentSet?
            .SelectMany(rds => registry.OfType<DocumentEntryDto>()
            .Where(de =>
                rds.DocumentUniqueId == de.UniqueId &&
                rds.HomeCommunityId == de.HomeCommunityId &&
                rds.RepositoryUniqueId == de.RepositoryUniqueId));

        var adhocQueryPatientId = soapEnvelope.Body.AdhocQueryRequest?.AdhocQuery?.GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId)?.GetFirstValue();
        var adhocQueryPatientValueJson = JsonSerializer.Serialize(SamlExtensions.GetSamlAttributeValueAsCodedValue(adhocQueryPatientId));

        switch (acp)
        {
            case Constants.Oid.Saml.Acp.NullValue:
                if (requestAppliesTo == Issuer.HelseId)
                {
                    if (soapAction == Constants.Xds.OperationContract.Iti18Action || soapAction == Constants.Xds.OperationContract.Iti38Action)
                    {
                        // If the resource in the SAML token is NOT the same as the patient ID in the AdhocQuery
                        if (resourceValueJson != providerValueJson || resourceValueJson != adhocQueryPatientValueJson)
                        {
                            result = XacmlContextDecision.Deny;
                            contextResult = new XacmlContextResult(result);
                            contextResult.Status = new XacmlContextStatus(new XacmlContextStatusCode(new Uri(Constants.Xacml.StatusCodes.Ok)));
                            contextResult.Status.StatusMessage = "SAML attribute 'resource-id' differs from AdhocQuery parameter '$XDSDocumentEntryPatientId'. User might be trying to leverage a BOLA vulnerability!";
                        }
                        else
                        {
                            result = XacmlContextDecision.Permit;
                            contextResult = new XacmlContextResult(result);
                        }
                    }

                    if (soapAction == Constants.Xds.OperationContract.Iti43Action && soapAction == Constants.Xds.OperationContract.Iti39Action)
                    {
                        var patientInfosFromEntries = documentEntries?.Select(de => JsonSerializer.Serialize(new CodedValue() { Code = de.SourcePatientInfo?.PatientId?.Id, CodeSystem = de.SourcePatientInfo?.PatientId?.System })) ?? [];
                        
                        // If they are trying to access a document for a patient thats not in the SAML-token
                        if (patientInfosFromEntries.Any(pinfo => pinfo != resourceValueJson))
                        {
                            result = XacmlContextDecision.Deny;
                            contextResult = new XacmlContextResult(result);
                            contextResult.Status = new XacmlContextStatus(new XacmlContextStatusCode(new Uri(Constants.Xacml.StatusCodes.Ok)));
                            contextResult.Status.StatusMessage = "SAML attribute 'resource-id' differs from the Patient Identifier specified in the Document Entries of the document(s) the user is trying to access!";
                        }
                    }
                }
                if (requestAppliesTo == Issuer.Helsenorge)
                {

                }
                break;

            case Constants.Oid.Saml.Acp.RepresentCitizenUnder12:
            case Constants.Oid.Saml.Acp.RepresentAnotherCitizen:
            case Constants.Oid.Saml.Acp.RepresentedUnableToConsent:
                // If the resource in the SAML token is NOT the same as the patient ID in the AdhocQuery
                if (resourceValueJson != adhocQueryPatientValueJson)
                {
                    result = XacmlContextDecision.Deny;
                    contextResult = new XacmlContextResult(result);
                    contextResult.Status = new XacmlContextStatus(new XacmlContextStatusCode(new Uri(Constants.Xacml.StatusCodes.Ok)));
                    contextResult.Status.StatusMessage = "SAML attribute 'resource-id' differs from AdhocQuery parameter '$XDSDocumentEntryPatientId'. User might be trying to leverage a BOLA vulnerability!";

                }
                else
                {
                    result = XacmlContextDecision.Permit;
                    contextResult = new XacmlContextResult(result);
                }
                break;
        }

        return new XacmlContextResponse(contextResult);
    }

    private XacmlContextResponse EvaluatePractitionerAdhocQuery(SoapEnvelope soapEnvelope, Saml2SecurityToken samlToken, string action)
    {
        throw new NotImplementedException();
    }

    private XacmlContextResponse EvaluateSoapRequestAndSamlToken(SoapEnvelope soapEnvelope, Saml2SecurityToken samlToken, string action, Issuer requestAppliesTo)
    {
        throw new NotImplementedException();
    }

    public XacmlContextResponse EvaluateXacmlRequest(XacmlContextRequest? xacmlRequest, Issuer xacmlRequestAppliesTo)
    {
        return _policyRepositoryWrapper.EvaluateRequest_V20(xacmlRequest, xacmlRequestAppliesTo);
    }
}
