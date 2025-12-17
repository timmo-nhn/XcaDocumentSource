using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.Win32;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Services;

/// <summary>
/// Parse incoming requests (ie. SOAP-requests with SAML-token) and generate XACML-access requests from the request assertions
/// </summary>
public static class PolicyRequestMapperSamlService
{

    public static Saml2SecurityToken ReadSamlToken(string inputSamlToken)
    {
        var handler = new Saml2SecurityTokenHandler();
        return handler.ReadSaml2Token(inputSamlToken);
    }

    public static XacmlContextRequest? GetXacmlRequestFromSamlToken(SoapEnvelope soapEnvelope, XacmlVersion xacmlVersion, Issuer appliesTo, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var samlToken = ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);
        return GetXacmlRequestFromSamlToken(soapEnvelope, samlToken, xacmlVersion, appliesTo, documentRegistry);
    }

    public static XacmlContextRequest? GetXacmlRequestFromSamlToken(string soapEnvelope, XacmlVersion xacmlVersion, Issuer appliesTo, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var sxmls = new SoapXmlSerializer();
        var soapEnvelopeObject = sxmls.DeserializeXmlString<SoapEnvelope>(soapEnvelope);

        var samlToken = ReadSamlToken(soapEnvelopeObject.Header.Security.Assertion?.OuterXml);
        return  GetXacmlRequestFromSamlToken(soapEnvelopeObject, samlToken, xacmlVersion, appliesTo, documentRegistry);
    }

    public static XacmlContextRequest? GetXacmlRequestFromSamlToken(SoapEnvelope soapEnvelope, Saml2SecurityToken samlToken, XacmlVersion xacmlVersion, Issuer appliesTo, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var action = MapXacmlActionFromSoapAction(soapEnvelope.Header.Action ?? string.Empty);

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

                // Resource
                var xacmlResourceAttribute = samlAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id")).ToList();

                xacmlResourceAttribute.AddRange(requestAttributes);

                var xacmlResource = new XacmlContextResource(xacmlResourceAttribute);

                var actionAttribute = new XacmlContextAttribute(
                    new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = xacmlActionString });

                var xacmlAction = new XacmlContextAction(actionAttribute);

                // Subject
                var subjectAttributes = samlAttributes.Where(sa => sa.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)) && (sa.AttributeId.OriginalString.Contains("subject")|| sa.AttributeId.OriginalString.Contains("acp"))).ToList();
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

    public static List<XacmlContextAttribute> MapRequestAttributesToXacml20Properties(SoapEnvelope soapEnvelope, IEnumerable<RegistryObjectDto>? documentRegistry = null)
    {
        var documentRequests = soapEnvelope.Body?.RetrieveDocumentSetRequest?.DocumentRequest;

        var adhocQueryPatientId = soapEnvelope.Body?.AdhocQueryRequest?.AdhocQuery?.GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId)?.GetFirstValue();
        var adhocQueryPatientValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(adhocQueryPatientId);

        var xacmlRequestAttributes = new List<XacmlContextAttribute>();
        
        if (adhocQueryPatientValue?.Code != null || adhocQueryPatientValue?.CodeSystem != null)
        {
            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri($"{Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier}:code"),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = adhocQueryPatientValue.Code }));

            xacmlRequestAttributes.Add(
                new XacmlContextAttribute(
                    new Uri($"{Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier}:codeSystem"),
                    new Uri(Constants.Xacml.DataType.String),
                    new XacmlContextAttributeValue() { Value = adhocQueryPatientValue.CodeSystem }));
        }


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

        return xacmlRequestAttributes;
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
                new XacmlContextAttributeValue() { Value = Constants.Oid.Saml.Acp.NullValue}));
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

    public static string MapXacmlActionFromSoapAction(string action)
    {
        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti38Action:
                return Constants.Xacml.Actions.ReadDocumentList;

            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti39Action:
                return Constants.Xacml.Actions.ReadDocuments;

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                return Constants.Xacml.Actions.Create;

            case Constants.Xds.OperationContract.Iti62Action:
            case Constants.Xds.OperationContract.Iti86Action:
                return Constants.Xacml.Actions.Delete;

            default:
                return Constants.Xacml.Actions.Unknown;
        }
    }
}
