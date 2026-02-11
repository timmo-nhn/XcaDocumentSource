using Abc.Xacml.Context;
using Abc.Xacml.Policy;
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

namespace XcaXds.Commons.Services;

/// <summary>
/// Parse incoming requests (ie. SOAP-requests with SAML-token) and generate XACML-access requests from the request assertions
/// </summary>
public static class PolicyRequestMapperSamlService
{

    public static Saml2SecurityToken? ReadSamlToken(string? inputSamlToken)
    {
        if (inputSamlToken == null) return null;

        var handler = new Saml2SecurityTokenHandler();
        return handler.ReadSaml2Token(inputSamlToken);
    }

    public static XacmlContextRequest? GetXacmlRequest(SoapEnvelope soapEnvelope, XacmlVersion xacmlVersion, Issuer appliesTo, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var samlToken = ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);
        return GetXacmlRequest(soapEnvelope, samlToken, xacmlVersion, appliesTo, documentRegistry);
    }

    public static XacmlContextRequest? GetXacmlRequest(string soapEnvelope, XacmlVersion xacmlVersion, Issuer appliesTo, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var sxmls = new SoapXmlSerializer();
        var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(soapEnvelope);

        var samlToken = ReadSamlToken(soapEnvelopeObject.Header.Security.Assertion?.OuterXml);
        return GetXacmlRequest(soapEnvelopeObject, samlToken, xacmlVersion, appliesTo, documentRegistry);
    }

    public static XacmlContextRequest? GetXacmlRequest(SoapEnvelope soapEnvelope, Saml2SecurityToken samlToken, XacmlVersion xacmlVersion, Issuer appliesTo, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var action = MapXacmlActionFromSoapEnvelope(soapEnvelope);

        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        if (appliesTo == Issuer.Unknown)
        {
            return null;
        }

        var samltokenAuthorizationAttributes = statements.Where(att =>
        att.Name.Contains("xacml") ||
        att.Name.Contains("xspa") ||
        att.Name.Contains("SecurityLevel") ||
        att.Name.Contains("Scope") ||
        att.Name.Contains("urn:ihe:iti") ||
        att.Name.Contains("acp") ||
        att.Name.Contains("provider-identifier"))
            .Append(new(Constants.Xacml.CustomAttributes.SamlNameId, samlToken.Assertion.Subject.NameId.Value));

        var xacmlAttributesList = new List<XacmlContextAttributes>();

        var xacmlActionString = action.ToString();

        XacmlContextRequest request;

        switch (xacmlVersion)
        {
            case XacmlVersion.Version20:
                var requestAttributes = MapRequestAttributesToXacml20Properties(soapEnvelope, documentRegistry);
                var samlAttributes = MapSamlAttributesToXacml20Properties(samltokenAuthorizationAttributes, xacmlActionString);
                var appliesToAttribute = MapAppliesToToXacml20Properties(appliesTo);

                // Resource
                var xacmlResourceAttribute = samlAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id")).ToList();

                xacmlResourceAttribute.AddRange(appliesToAttribute);
                xacmlResourceAttribute.AddRange(requestAttributes);

                var xacmlResource = new XacmlContextResource(xacmlResourceAttribute);

                var actionAttribute = new XacmlContextAttribute(
                    new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = xacmlActionString });

                var xacmlAction = new XacmlContextAction(actionAttribute);

                // Subject
                var subjectAttributes = samlAttributes.Where(sa => sa.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)) && (sa.AttributeId.OriginalString.Contains("subject") || sa.AttributeId.OriginalString.Contains("acp"))).ToList();
                subjectAttributes.AddRange(requestAttributes);
                var xacmlSubject = new XacmlContextSubject(subjectAttributes);

                // Environment
                var xacmlEnvironment = new XacmlContextEnvironment();

                request = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject, xacmlEnvironment);

                return request;


            case XacmlVersion.Version30:
                var xacmlAllAttributes = MapSamlAttributesToXacml30Properties(samltokenAuthorizationAttributes, xacmlActionString);

                var xacmlActionAttribute = new XacmlAttribute(new Uri(Constants.Xacml.Attribute.ActionId), false);
                var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), xacmlActionString);
                xacmlActionAttribute.AttributeValues.Add(xacmlActionAttributeValue);
                xacmlAllAttributes.Add(xacmlActionAttribute);

                var xacmlSubjectContextAttributes = new XacmlContextAttributes(
                    new Uri(Constants.Xacml.Category.V30_Subject),
                    xacmlAllAttributes.Where(xatt => xatt.AttributeId.AbsolutePath.Contains("subject")));

                var xacmlResourceContextAttributes = new XacmlContextAttributes(
                    new Uri(Constants.Xacml.Category.V30_Resource),
                    xacmlAllAttributes.Where(xatt => xatt.AttributeId.AbsolutePath.Contains("resource-id")));

                var xacmlActionContextAttributes = new XacmlContextAttributes(
                    new Uri(Constants.Xacml.Category.V30_Action),
                    xacmlAllAttributes.Where(xatt => xatt.AttributeId.AbsolutePath.Contains("action-id")));

                var xacmlEnvironmentContextAttributes = new XacmlContextAttributes(
                    new Uri(Constants.Xacml.Category.V30_Environment),
                    Enumerable.Empty<XacmlAttribute>());

                xacmlAttributesList.Add(xacmlSubjectContextAttributes);
                xacmlAttributesList.Add(xacmlResourceContextAttributes);
                xacmlAttributesList.Add(xacmlActionContextAttributes);
                xacmlAttributesList.Add(xacmlEnvironmentContextAttributes);

                request = new XacmlContextRequest(false, false, xacmlAttributesList);
                request.ReturnPolicyIdList = false;
                request.CombinedDecision = false;

                return request;


            default:
                return null;
        }
    }


    public static Issuer GetIssuerEnumFromSamlTokenIssuer(string value)
    {
        if (value.Contains("helseid"))
        {
            return Issuer.HelseId;
        }
        if (value.Contains("helsenorge"))
        {
            return Issuer.Helsenorge;
        }
        return Issuer.Unknown;
    }

    private static List<XacmlContextAttribute> MapAppliesToToXacml20Properties(Issuer appliesTo)
    {
        var xacmlAttributes = new List<XacmlContextAttribute>();
        switch (appliesTo)
        {
            case Issuer.Helsenorge:
                xacmlAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Xacml.CustomAttributes.AppliesTo),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = appliesTo.ToString() }));
                break;
            case Issuer.HelseId:
                xacmlAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Xacml.CustomAttributes.AppliesTo),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = appliesTo.ToString() }));
                break;
            default:
                break;
        }
        return xacmlAttributes;
    }

    public static List<XacmlContextAttribute> MapRequestAttributesToXacml20Properties(SoapEnvelope soapEnvelope, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        // ReadDocumentList
        var adhocQueryPatientId = soapEnvelope.Body?.AdhocQueryRequest?.AdhocQuery?.GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId)?.GetFirstValue();
        var adhocQueryPatientValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(adhocQueryPatientId);

        // ReadDocuments
        var documentRequests = soapEnvelope.Body?.RetrieveDocumentSetRequest?.DocumentRequest;

        // Create
        var provideAndRegisterRequest = soapEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList ?? soapEnvelope.Body?.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;

        // Delete
        var removeObjectsRequest = soapEnvelope.Body?.RemoveObjectsRequest?.ObjectRefList?.ObjectRef;
        var removeDocumentsRequest = soapEnvelope.Body?.RemoveDocumentsRequest?.DocumentRequest;

        var xacmlRequestAttributes = new List<XacmlContextAttribute>();

        MapRequestAttributesFromAdhocQueryRequest(xacmlRequestAttributes, adhocQueryPatientValue);
        MapRequestAttributesFromRetrieveDocumentSet(xacmlRequestAttributes, documentRequests, documentRegistry);
        MapRequestAttributesFromProvideAndRegisterRequest(xacmlRequestAttributes, provideAndRegisterRequest);
        MapRequestAttributesFromRemoveObjectsRequest(xacmlRequestAttributes, removeObjectsRequest);
        MapRequestAttributesFromRemoveDocumentsRequest(xacmlRequestAttributes, removeDocumentsRequest, documentRegistry);

        return xacmlRequestAttributes;
    }

    private static void MapRequestAttributesFromRemoveObjectsRequest(List<XacmlContextAttribute> xacmlRequestAttributes, IdentifiableType[]? removeObjectsRequest)
    {
        foreach (var removeObject in removeObjectsRequest ?? [])
        {
            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri(Constants.Xacml.CustomAttributes.DocumentUniqueId),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = removeObject.Id }));
        }
    }

    private static void MapRequestAttributesFromRemoveDocumentsRequest(List<XacmlContextAttribute> xacmlRequestAttributes, DocumentRequestType[]? removeDocumentsRequest, IEnumerable<RegistryObjectDto>? documentRegistry)
    {
        MapRequestAttributesFromRetrieveDocumentSet(xacmlRequestAttributes, removeDocumentsRequest, documentRegistry);
    }

    private static void MapRequestAttributesFromProvideAndRegisterRequest(List<XacmlContextAttribute> xacmlRequestAttributes, IdentifiableType[]? provideAndRegisterRequest)
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
                    new Uri(Constants.Xacml.CustomAttributes.RepositoryUniqueId),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = registryRepository.Repository }));

            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri(Constants.Xacml.CustomAttributes.HomeCommunityId),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = registryRepository.HomeCommunity }));
        }
    }

    private static void MapRequestAttributesFromRetrieveDocumentSet(List<XacmlContextAttribute> xacmlRequestAttributes, DocumentRequestType[]? documentRequests, IEnumerable<RegistryObjectDto>? documentRegistry)
    {
        foreach (var documentRequest in documentRequests ?? [])
        {
            if (documentRequest.DocumentUniqueId != null)
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Xacml.CustomAttributes.DocumentUniqueId),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentRequest.DocumentUniqueId }));
            }

            if (documentRequest.RepositoryUniqueId != null)
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Xacml.CustomAttributes.HomeCommunityId),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentRequest.HomeCommunityId }));
            }

            if (documentRequest.HomeCommunityId != null)
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri(Constants.Xacml.CustomAttributes.RepositoryUniqueId),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentRequest.RepositoryUniqueId }));
            }

            var documentEntryForDocument = documentRegistry?.OfType<DocumentEntryDto>()
                .FirstOrDefault(de =>
                de.UniqueId == documentRequest.DocumentUniqueId &&
                de.RepositoryUniqueId == documentRequest.RepositoryUniqueId &&
                de.HomeCommunityId == documentRequest.HomeCommunityId);

            if (!string.IsNullOrWhiteSpace(documentEntryForDocument?.SourcePatientInfo?.PatientId?.Id))
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri($"{Constants.Xacml.CustomAttributes.DocumentEntryPatientIdentifier}:code"),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentEntryForDocument.SourcePatientInfo.PatientId.Id }));
            }

            if (!string.IsNullOrWhiteSpace(documentEntryForDocument?.SourcePatientInfo?.PatientId?.System))
            {
                xacmlRequestAttributes.Add(
                    new XacmlContextAttribute(
                        new Uri($"{Constants.Xacml.CustomAttributes.DocumentEntryPatientIdentifier}:codeSystem"),
                        new Uri(Constants.Xacml.DataType.String),
                        new XacmlContextAttributeValue() { Value = documentEntryForDocument.SourcePatientInfo.PatientId.System }));
            }
        }
    }

    private static void MapRequestAttributesFromAdhocQueryRequest(List<XacmlContextAttribute> xacmlRequestAttributes, CodedValue? patientIdentifier)
    {
        if (patientIdentifier?.Code != null || patientIdentifier?.CodeSystem != null)
        {
            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri($"{Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier}:code"),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = patientIdentifier.Code }));

            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri($"{Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier}:codeSystem"),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = patientIdentifier.CodeSystem }));
        }
    }

    private static List<XacmlAttribute> MapSamlAttributesToXacml30Properties(IEnumerable<Saml2Attribute> samltokenAuthorizationAttributes, string action)
    {
        var xacmlAllAttributes = new List<XacmlAttribute>();
        // SAML token values, map to XACML values
        foreach (var samlAttribute in samltokenAuthorizationAttributes)
        {
            foreach (var attributeValue in samlAttribute.Values)
            {
                var attributeValueAsCodedValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(attributeValue);

                // If its structured codedvalue format or just plain text
                if (attributeValueAsCodedValue != null)
                {
                    if (attributeValueAsCodedValue.Code != null)
                    {

                        var xacmlAttribute = new XacmlAttribute(new Uri(samlAttribute.Name + ":code"), false);
                        var xacmlAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), attributeValueAsCodedValue.Code);
                        xacmlAttribute.AttributeValues.Add(xacmlAttributeValue);
                        xacmlAllAttributes.Add(xacmlAttribute);

                    }

                    if (attributeValueAsCodedValue.CodeSystem != null)
                    {
                        var xacmlAttribute = new XacmlAttribute(new Uri(samlAttribute.Name + ":codeSystem"), false);
                        var xacmlAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), attributeValueAsCodedValue.CodeSystem);
                        xacmlAttribute.AttributeValues.Add(xacmlAttributeValue);
                        xacmlAllAttributes.Add(xacmlAttribute);
                    }

                    if (attributeValueAsCodedValue.DisplayName != null)
                    {
                        var xacmlAttribute = new XacmlAttribute(new Uri(samlAttribute.Name + ":displayName"), false);
                        var xacmlAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), attributeValueAsCodedValue.DisplayName);
                        xacmlAttribute.AttributeValues.Add(xacmlAttributeValue);
                        xacmlAllAttributes.Add(xacmlAttribute);
                    }
                }
                else
                {
                    var xacmlAttribute = new XacmlAttribute(new Uri(samlAttribute.Name + ":displayName"), false);
                    var xacmlAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), attributeValue);
                    xacmlAttribute.AttributeValues.Add(xacmlAttributeValue);
                    xacmlAllAttributes.Add(xacmlAttribute);
                }
            }
        }

        return xacmlAllAttributes;
    }

    public static List<XacmlContextAttribute> MapSamlAttributesToXacml20Properties(IEnumerable<Saml2Attribute> samltokenAuthorizationAttributes, string action)
    {
        var subjectAttributes = new List<XacmlContextAttribute>();

        foreach (var attribute in samltokenAuthorizationAttributes)
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

                // If its structured codedvalue format or just plain text
                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue.Code) &&
                    string.IsNullOrWhiteSpace(attributeValueAsCodedValue.CodeSystem) &&
                    string.IsNullOrWhiteSpace(attributeValueAsCodedValue.DisplayName))
                {
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.Code }));
                }
                else
                {
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name + ":code"),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.Code }));
                }

                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue.CodeSystem))
                {
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name + ":codeSystem"),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.CodeSystem }));

                }

                if (!string.IsNullOrWhiteSpace(attributeValueAsCodedValue.DisplayName))
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

        return subjectAttributes;
    }


    public static string? GetActionFromSoapEnvelopeString(string? inputSoapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(inputSoapEnvelope);

        return soapEnvelopeObject.Header.Action;
    }

    public static string? GetSamlTokenFromSoapEnvelope(string inputSoapEnvelope)
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
        switch (soapEnvelope?.Header?.Action)
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
        var registryObjects = soapEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;

        var isReplaceUpdate = registryObjects?.OfType<AssociationType>().Any(assoc => assoc.AssociationTypeData.IsAnyOf(Replace, Transformation, Addendum, ReplaceWithTransformation)) ?? false;
        return isReplaceUpdate ? Constants.Xacml.Actions.Update : Constants.Xacml.Actions.Create;
    }
}
