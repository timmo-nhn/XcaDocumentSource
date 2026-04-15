using Abc.Xacml.Context;
using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using static XcaXds.Commons.Commons.Constants.Xds.AssociationType;

namespace XcaXds.WebService.Services;

/// <summary>
/// Parse incoming requests (ie. SOAP-requests with SAML-token) and generate XACML 2.0 access requests from the request assertions
/// </summary>
public class PolicyRequestMapperSamlService
{
    private readonly RegistryWrapper _registryWrapper;
    public PolicyRequestMapperSamlService(RegistryWrapper registryWrapper)
    {
        _registryWrapper = registryWrapper;
    }

    public XacmlContextRequest? GetXacmlRequest(SoapEnvelope soapEnvelope)
    {
        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelope.Header.Security?.Assertion?.OuterXml);
        return GetXacmlRequest(soapEnvelope, samlToken);
    }

    public XacmlContextRequest? GetXacmlRequest(string soapEnvelope)
    {
        var sxmls = new SoapXmlSerializer();
        var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(soapEnvelope);

        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelopeObject.Header.Security?.Assertion?.OuterXml);
        return GetXacmlRequest(soapEnvelopeObject, samlToken);
    }

    public XacmlContextRequest? GetXacmlRequest(SoapEnvelope soapEnvelope, Saml2SecurityToken? samlToken)
    {
        var action = MapXacmlActionFromSoapEnvelope(soapEnvelope);
        var appliesTo = SamlExtensions.GetIssuerEnumFromSamlToken(samlToken);

        var statements = samlToken?.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

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

        var xacmlAttributesList = new List<XacmlContextAttributes>();

        var xacmlActionString = action.ToString();

        var requestAttributes = MapRequestAttributesToXacml20Properties(soapEnvelope);
        var samlAttributes = MapSamlAttributesToXacml20Properties(samltokenAuthorizationAttributes, xacmlActionString);

        // Resource
        var xacmlResourceAttribute = samlAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id")).ToList();

        xacmlResourceAttribute.AddRange(requestAttributes);

        var xacmlResource = new XacmlContextResource(xacmlResourceAttribute);

        var actionAttribute = new XacmlContextAttribute(
            new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = xacmlActionString });

        var xacmlAction = new XacmlContextAction(actionAttribute);

        // Subject
        var appliesToAttribute = MapAppliesToToXacml20Properties(appliesTo);
        
        var subjectAttributes = samlAttributes
            .Where(sa => sa.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)) &&
                        (sa.AttributeId.OriginalString.Contains("subject") ||
                            sa.AttributeId.OriginalString.Contains("acp")))
            .ToList();

        subjectAttributes.AddRange(requestAttributes);
        subjectAttributes.AddRange(appliesToAttribute);

        var xacmlSubject = new XacmlContextSubject(subjectAttributes);

        // Environment
        var xacmlEnvironment = new XacmlContextEnvironment();

        var request = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject, xacmlEnvironment);

        return request;
    }

    public static List<XacmlContextAttribute> MapAppliesToToXacml20Properties(AppliesTo appliesTo)
    {
        var xacmlAttributes = new List<XacmlContextAttribute>
        {
            new XacmlContextAttribute(
                new Uri(Constants.Urn.Custom.AppliesTo),
                new Uri(Constants.Xacml.DataType.String),
                new XacmlContextAttributeValue() { Value = appliesTo.ToString() })
        };

        return xacmlAttributes;
    }

    public List<XacmlContextAttribute> MapRequestAttributesToXacml20Properties(SoapEnvelope soapEnvelope)
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

        var xacmlRequestAttributes = new List<XacmlContextAttribute>();

        MapRequestAttributesFromAdhocQueryRequest(xacmlRequestAttributes, adhocQueryPatientValue);
        MapRequestAttributesFromRetrieveDocumentSet(xacmlRequestAttributes, documentRequests);
        MapRequestAttributesFromProvideAndRegisterRequest(xacmlRequestAttributes, provideAndRegisterRequest);
        MapRequestAttributesFromRemoveObjectsRequest(xacmlRequestAttributes, removeObjectsRequest);
        MapRequestAttributesFromRemoveDocumentsRequest(xacmlRequestAttributes, removeDocumentsRequest);

        return xacmlRequestAttributes;
    }

    private void MapRequestAttributesFromRemoveObjectsRequest(List<XacmlContextAttribute> xacmlRequestAttributes, IdentifiableType[]? removeObjectsRequest)
    {
        foreach (var removeObject in removeObjectsRequest ?? [])
        {
            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri(Constants.Urn.Custom.DocumentUniqueId),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = removeObject.Id }));
        }
    }

    private void MapRequestAttributesFromRemoveDocumentsRequest(List<XacmlContextAttribute> xacmlRequestAttributes, DocumentRequestType[]? removeDocumentsRequest)
    {
        MapRequestAttributesFromRetrieveDocumentSet(xacmlRequestAttributes, removeDocumentsRequest);
    }

    private void MapRequestAttributesFromProvideAndRegisterRequest(List<XacmlContextAttribute> xacmlRequestAttributes, IdentifiableType[]? provideAndRegisterRequest)
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
            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri(Constants.Urn.Custom.RepositoryUniqueId),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = registryRepository.Repository }));

            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri(Constants.Urn.Custom.HomeCommunityId),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = registryRepository.HomeCommunity }));
        }
    }

    private void MapRequestAttributesFromRetrieveDocumentSet(List<XacmlContextAttribute> xacmlRequestAttributes, DocumentRequestType[]? documentRequests)
    {

        foreach (var documentRequest in documentRequests ?? [])
        {
            if (documentRequest.DocumentUniqueId != null)
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Urn.Custom.DocumentUniqueId),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentRequest.DocumentUniqueId }));
            }

            if (documentRequest.RepositoryUniqueId != null)
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Urn.Custom.HomeCommunityId),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentRequest.HomeCommunityId }));
            }

            if (documentRequest.HomeCommunityId != null)
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Urn.Custom.RepositoryUniqueId),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentRequest.RepositoryUniqueId }));
            }

            var documentRegistry = _registryWrapper.GetSingleRegistryObjectAsDto(documentRequest.DocumentUniqueId ?? "");

            var documentEntryForDocument = documentRegistry as DocumentEntryDto;

            if (!string.IsNullOrWhiteSpace(documentEntryForDocument?.SourcePatientInfo?.PatientId?.Id))
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri($"{Constants.Urn.Custom.DocumentEntryPatientIdentifier}:code"),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentEntryForDocument.SourcePatientInfo.PatientId.Id }));
            }

            if (!string.IsNullOrWhiteSpace(documentEntryForDocument?.SourcePatientInfo?.PatientId?.System))
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri($"{Constants.Urn.Custom.DocumentEntryPatientIdentifier}:codeSystem"),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentEntryForDocument.SourcePatientInfo.PatientId.System }));
            }
        }
    }

    private void MapRequestAttributesFromAdhocQueryRequest(List<XacmlContextAttribute> xacmlRequestAttributes, CodedValue? patientIdentifier)
    {
        if (patientIdentifier?.Code != null || patientIdentifier?.CodeSystem != null)
        {
            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri($"{Constants.Urn.Custom.AdhocQueryPatientIdentifier}:code"),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = patientIdentifier.Code }));

            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri($"{Constants.Urn.Custom.AdhocQueryPatientIdentifier}:codeSystem"),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = patientIdentifier.CodeSystem }));
        }
    }

    public static List<XacmlContextAttribute> MapSamlAttributesToXacml20Properties(IEnumerable<Saml2Attribute>? samltokenAuthorizationAttributes, string action)
    {
        var subjectAttributes = new List<XacmlContextAttribute>();

        foreach (var attribute in samltokenAuthorizationAttributes ?? [])
        {
            var attributeValue = attribute.Values.FirstOrDefault(); // Never have i ever: seen a SAML-AttributeStatement with more than one Value
            if (attributeValue == null) continue;

            var attributeValueAsCodedValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(attributeValue);

            try
            {
                // If-statements to fix Helsenorge STS values not being proper GUIDs
                if (attribute.Name.Contains("SecurityLevel"))
                {
                    attribute.Name = "urn:no:ehelse:saml:1.0:subject:SecurityLevel";
                }
                if (attribute.Name.Contains("Scope"))
                {
                    attribute.Name = "urn:no:ehelse:saml:1.0:subject:Scope";
                }
                if (!Uri.TryCreate(attribute.Name, UriKind.Absolute, out _))
                {
                    attribute.Name = Constants.Urn.Custom.UnknownAttribute + ":" + attribute.Name;
                }

                if (!Uri.IsWellFormedUriString(attribute.Name, UriKind.Absolute))
                {
                    // Skip the following from HelseID user tokens: 
                    //  - name
                    //  - family_name
                    //  - given_name

                    // and potentially others that are not in URI format, as XACML 2.0 requires AttributeIds to be URIs.

                    continue;
                }

                // If its structured codedvalue format or just plain text
                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.Code) &&
                    string.IsNullOrWhiteSpace(attributeValueAsCodedValue.CodeSystem) &&
                    string.IsNullOrWhiteSpace(attributeValueAsCodedValue.DisplayName))
                {
                    var attributeValuesToAdd = attributeValueAsCodedValue.Code.Split(",");

                    var attributeValues = new List<XacmlContextAttributeValue>();

                    foreach (var otherAttributeValues in attributeValuesToAdd)
                    {
                        attributeValues.Add(new XacmlContextAttributeValue() { Value = otherAttributeValues });
                    }

                    subjectAttributes.Add(new XacmlContextAttribute(
                        new Uri(attribute.Name),
                        new Uri(Constants.Xacml.DataType.String),
                        attributeValues));
                }
                else
                {
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name + ":code"),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue?.Code }));
                }

                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.CodeSystem))
                {
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name + ":codeSystem"),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.CodeSystem }));
                }

                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue?.DisplayName))
                {
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name + ":displayName"),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.DisplayName }));
                }
            }
            catch (UriFormatException urix)
            {
                throw new InvalidOperationException(
                    $"Invalid URI in attribute: {attribute.Name}", urix);
            }
        }

        if (subjectAttributes.Any(att => att.AttributeId.ToString() == Constants.Saml.Attribute.XuaAcp) == false)
        {
            subjectAttributes.Add(
            new XacmlContextAttribute(
                new Uri(Constants.Saml.Attribute.XuaAcp + ":code"),
                new Uri(Constants.Xacml.DataType.String),
                new XacmlContextAttributeValue() { Value = Constants.Oid.Saml.Acp.NullValue }));
        }

        return [.. subjectAttributes.DistinctBy(att => new { att.AttributeId, AttributeValues = string.Join(", ", att.AttributeValues) })];
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

    public static string MapXacmlActionFromSoapEnvelope(SoapEnvelope soapEnvelope)
    {
        switch (soapEnvelope?.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti38Action:
                return Constants.Xacml.Actions.ReadDocumentList;

            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti39Action:
                return Constants.Xacml.Actions.ReadDocuments;

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                return GetCreateOrUpdateFromRequest(soapEnvelope);

            case Constants.Xds.OperationContract.Iti62Action:
            case Constants.Xds.OperationContract.Iti86Action:
                return Constants.Xacml.Actions.Delete;

            default:
                return Constants.Xacml.Actions.Unknown;
        }
    }

    private static string GetCreateOrUpdateFromRequest(SoapEnvelope soapEnvelope)
    {
        var registryObjects = soapEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        var isReplaceUpdate = registryObjects?.OfType<AssociationType>().Any(assoc => assoc.AssociationTypeData?.IsAnyOf(Replace, Transformation, Addendum, ReplaceWithTransformation) == true) ?? false;
        return isReplaceUpdate ? Constants.Xacml.Actions.Update : Constants.Xacml.Actions.Create;
    }
}
