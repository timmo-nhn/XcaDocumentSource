using Microsoft.IdentityModel.Tokens.Saml2;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;
using XcaXds.WebService.Services.XdsRegistry;

namespace XcaXds.WebService.Services.Policy;

/// <summary>
/// Parse incoming requests (i.e. SOAP-requests with SAML-token) and generate Access requests from the request assertions
/// </summary
// HAYO! Maybe refactor into Interface!!!
public class PolicyRequestMapperSamlService
{
    private readonly ILogger<PolicyRequestMapperSamlService> _logger;
    private readonly RegistryWrapper _registryWrapper;
    private readonly TerminologyService _terminologyService;

    public PolicyRequestMapperSamlService(
        ILogger<PolicyRequestMapperSamlService> logger,
        RegistryWrapper registryWrapper,
        TerminologyService terminologyService)
    {
        _logger = logger;
        _registryWrapper = registryWrapper;
        _terminologyService = terminologyService;
    }

    public AbacRequest? GetAbacRequestFromSoapEnvelope(SoapEnvelope soapEnvelope)
    {
        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelope.Header.Security?.Assertion?.OuterXml);
        return GetAbacRequestFromSoapEnvelope(soapEnvelope, samlToken);
    }

    public AbacRequest? GetAbacRequestFromSoapEnvelope(string soapEnvelope)
    {
        var sxmls = new SoapXmlSerializer();
        var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(soapEnvelope);

        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelopeObject.Header.Security?.Assertion?.OuterXml);
        return GetAbacRequestFromSoapEnvelope(soapEnvelopeObject, samlToken);
    }

    public AbacRequest? GetAbacRequestFromSoapEnvelope(SoapEnvelope soapEnvelope, Saml2SecurityToken? samlToken)
    {
        var abacRequest = new AbacRequest();

        var action = AccessControlExtensions.MapXacmlActionFromSoapEnvelope(soapEnvelope);
        var appliesTo = SamlExtensions.GetIssuerEnumFromSamlToken(samlToken);

        var statements = samlToken?.GetAllStatements();

        if (appliesTo == AppliesTo.Unknown)
        {
            return null;
        }

        var samltokenAuthorizationAttributes = statements?
            .Where(att =>
                att.Name.Contains("xacml") ||
                att.Name.Contains("xspa") ||
                att.Name.Contains("SecurityLevel") ||
                att.Name.Contains("Scope") ||
                att.Name.Contains("urn:ihe:iti") ||
                att.Name.Contains("acp") ||
                att.Name.Contains("provider-identifier"))
            .Append(new(Constants.Urn.Custom.SamlNameId, samlToken?.Assertion.Subject.NameId.Value));

        var requestAttributes = MapRequestAttributesToAbacProperties(soapEnvelope);
        var samlAttributes = MapSamlAttributesToAbacProperties(samltokenAuthorizationAttributes);

        var appliesToAttribute = MapAppliesToToAbacProperties(appliesTo);

        abacRequest.Attributes.AddRange(requestAttributes);
        abacRequest.Attributes.AddRange(samlAttributes);
        abacRequest.Attributes.AddOrUpdate(Constants.Xacml.Attribute.ActionId, [action]);
        abacRequest.Attributes.AddRange(appliesToAttribute);

        return abacRequest;
    }

    public Dictionary<string, List<string>> MapAppliesToToAbacProperties(AppliesTo appliesTo)
    {
        return new()
        {
            [Constants.Urn.Custom.AppliesTo] = [appliesTo.ToString()]
        };
    }

    public Dictionary<string, List<string>> MapRequestAttributesToAbacProperties(SoapEnvelope soapEnvelope)
    {
        // ReadDocumentList
        var adhocQueryPatientId = soapEnvelope.Body.AdhocQueryRequest?.AdhocQuery?.GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId)?.GetFirstValue();
        var adhocQueryPatientValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(adhocQueryPatientId);

        // ReadDocuments
        var documentRequests = soapEnvelope.Body.RetrieveDocumentSetRequest?.DocumentRequest;

        // Create
        var provideAndRegisterRequest = soapEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList ?? soapEnvelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        // Delete
        var removeObjectsRequest = soapEnvelope.Body.RemoveObjectsRequest?.ObjectRefList?.ObjectRef;
        var removeDocumentsRequest = soapEnvelope.Body.RemoveDocumentsRequest?.DocumentRequest;

        var abacRequestAttributes = new Dictionary<string, List<string>>();

        MapRequestAttributesFromAdhocQueryRequest(abacRequestAttributes, adhocQueryPatientValue);
        MapRequestAttributesFromRetrieveDocumentSet(abacRequestAttributes, documentRequests);
        MapRequestAttributesFromProvideAndRegisterRequest(abacRequestAttributes, provideAndRegisterRequest);
        MapRequestAttributesFromRemoveObjectsRequest(abacRequestAttributes, removeObjectsRequest);
        MapRequestAttributesFromRemoveDocumentsRequest(abacRequestAttributes, removeDocumentsRequest);

        return abacRequestAttributes;
    }

    private void MapRequestAttributesFromRemoveObjectsRequest(Dictionary<string, List<string>> abacRequestAttributes, IdentifiableType[]? removeObjectsRequest)
    {
        foreach (var removeObject in removeObjectsRequest ?? [])
        {
            if(removeObject.Id != null)
                abacRequestAttributes.AddOrUpdate(Constants.Urn.Custom.DocumentUniqueId, [removeObject.Id]);
        }
    }

    private void MapRequestAttributesFromRemoveDocumentsRequest(Dictionary<string, List<string>> xacmlRequestAttributes, DocumentRequestType[]? removeDocumentsRequest)
    {
        MapRequestAttributesFromRetrieveDocumentSet(xacmlRequestAttributes, removeDocumentsRequest);
    }

    private void MapRequestAttributesFromProvideAndRegisterRequest(Dictionary<string, List<string>> abacRequestAttributes, IdentifiableType[]? provideAndRegisterRequest)
    {
        var registriesRepositoriesToUploadTo = provideAndRegisterRequest?
            .OfType<ExtrinsicObjectType>()
            .Select(eo =>
                new
                {
                    HomeCommunity = eo.Home,
                    Repository = eo.GetFirstSlot(Constants.Xds.SlotNames.RepositoryUniqueId)?.GetFirstValue()
                })
            .Distinct().ToArray();

        foreach (var registryRepository in registriesRepositoriesToUploadTo ?? [])
        {
            if (registryRepository.Repository != null)
                abacRequestAttributes.AddOrUpdate(Constants.Urn.Custom.RepositoryUniqueId, [registryRepository.Repository]);
            if (registryRepository.HomeCommunity != null)
                abacRequestAttributes.AddOrUpdate(Constants.Urn.Custom.HomeCommunityId, [registryRepository.HomeCommunity]);
        }
    }

    private void MapRequestAttributesFromRetrieveDocumentSet(Dictionary<string, List<string>> abacRequestAttributes, DocumentRequestType[]? documentRequests)
    {
        foreach (var documentRequest in documentRequests ?? [])
        {
            if (documentRequest.DocumentUniqueId != null)
            {
                abacRequestAttributes.AddOrUpdate(Constants.Urn.Custom.DocumentUniqueId, [documentRequest.DocumentUniqueId]);
            }

            if (documentRequest.HomeCommunityId != null)
            {
                abacRequestAttributes.AddOrUpdate(Constants.Urn.Custom.HomeCommunityId, [documentRequest.HomeCommunityId]);
            }

            if (documentRequest.RepositoryUniqueId != null)
            {
                abacRequestAttributes.AddOrUpdate(Constants.Urn.Custom.RepositoryUniqueId, [documentRequest.RepositoryUniqueId]);
            }

            var documentRegistry = _registryWrapper.GetSingleRegistryObjectAsDto(documentRequest.DocumentUniqueId ?? "");

            var documentEntryForDocument = documentRegistry as DocumentEntryDto;

            if (!string.IsNullOrWhiteSpace(documentEntryForDocument?.SourcePatientInfo?.PatientId?.Id))
            {
                abacRequestAttributes.AddOrUpdate($"{Constants.Urn.Custom.DocumentEntryPatientIdentifier}:code", [documentEntryForDocument.SourcePatientInfo.PatientId.Id]);
            }

            if (!string.IsNullOrWhiteSpace(documentEntryForDocument?.SourcePatientInfo?.PatientId?.System))
            {
                abacRequestAttributes.AddOrUpdate($"{Constants.Urn.Custom.DocumentEntryPatientIdentifier}:codeSystem", [documentEntryForDocument.SourcePatientInfo.PatientId.System]);
            }
        }
    }

    private void MapRequestAttributesFromAdhocQueryRequest(Dictionary<string, List<string>> abacRequestAttributes, CodedValue? patientIdentifier)
    {
        if (patientIdentifier?.Code != null || patientIdentifier?.CodeSystem != null)
        {
            if (patientIdentifier.Code != null)
                abacRequestAttributes.AddOrUpdate($"{Constants.Urn.Custom.AdhocQueryPatientIdentifier}:code", [patientIdentifier.Code]);
            if (patientIdentifier.CodeSystem != null)
                abacRequestAttributes.AddOrUpdate($"{Constants.Urn.Custom.AdhocQueryPatientIdentifier}:codeSystem", [patientIdentifier.CodeSystem]);
        }
    }

    public Dictionary<string, List<string>> MapSamlAttributesToAbacProperties(IEnumerable<Saml2Attribute>? samltokenAuthorizationAttributes)
    {
        var abacProperties = new Dictionary<string, List<string>>();

        foreach (var attribute in samltokenAuthorizationAttributes ?? [])
        {
            var attributeValue = attribute.Values.FirstOrDefault();
            if (attributeValue == null) continue;

            var attributeValueAsCodedValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(attributeValue);

            try
            {
                // If-statements to fix Helsenorge STS values not being proper GUIDs
                if (attribute.Name.Contains("SecurityLevel"))
                {
                    var securityLevel = _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Authentication.SamlAttributes, "SecurityLevel")?.FirstOrDefault();

                    attribute.Name = securityLevel;
                }

                if (attribute.Name?.Contains("Scope") == true)
                {
                    attribute.Name = Constants.Saml.Attribute.EhelseScope;
                }

                if (!Uri.TryCreate(attribute.Name, UriKind.Absolute, out _))
                {
                    attribute.Name = Constants.Urn.Custom.UnknownAttribute + ":" + attribute.Name;
                }

                // If its structured codedvalue format or just plain text
                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.Code) &&
                    string.IsNullOrWhiteSpace(attributeValueAsCodedValue.CodeSystem) &&
                    string.IsNullOrWhiteSpace(attributeValueAsCodedValue.DisplayName))
                {
                    var attributeValuesToAdd = attributeValueAsCodedValue.Code.Split(",").ToList();

                    abacProperties.AddOrUpdate(attribute.Name, attributeValuesToAdd);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.Code))
                        abacProperties.AddOrUpdate(attribute.Name + ":code", [attributeValueAsCodedValue.Code]);
                }

                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.CodeSystem))
                {
                    abacProperties.AddOrUpdate(attribute.Name + ":codeSystem", [attributeValueAsCodedValue.CodeSystem]);
                }

                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.DisplayName))
                {
                    abacProperties.AddOrUpdate(attribute.Name + ":displayName", [attributeValueAsCodedValue.DisplayName]);
                }
            }
            catch (UriFormatException urix)
            {
                throw new InvalidOperationException(
                    $"Invalid URI in attribute: {attribute.Name}", urix);
            }
        }

        if (abacProperties.All(att => att.Key.ToString() != Constants.Saml.Attribute.XuaAcp))
        {
            // Add default ACP "null value"
            var acpNullValue = _terminologyService.GetValueFromCodeSystemByName(CodeSystemNames.Authentication.Acp, "NullValue")?.FirstOrDefault();
            if (acpNullValue != null)
                abacProperties.AddOrUpdate(Constants.Saml.Attribute.XuaAcp, [acpNullValue]);
        }

        return abacProperties;
    }


    public static string? GetActionFromSoapEnvelopeString(string? inputSoapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(inputSoapEnvelope);

        return soapEnvelopeObject.Header.Action;
    }

    public string? GetSamlTokenFromSoapEnvelope(string inputSoapEnvelope)
    {
        var soapEnvelopeXmlDocument = new XmlDocument();
        try
        {
            soapEnvelopeXmlDocument.LoadXml(inputSoapEnvelope);
        }
        catch (Exception)
        {
            return null;
        }


        var assertion = soapEnvelopeXmlDocument.GetElementsByTagName("saml:Assertion");

        if (assertion.Count == 0)
        {
            return null;
        }

        return assertion[0]?.OuterXml;
    }
}