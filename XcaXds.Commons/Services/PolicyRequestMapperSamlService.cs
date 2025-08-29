using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
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

    public static async Task<XacmlContextRequest> GetXacmlRequestFromSamlToken(string inputSamlToken, string action, XacmlVersion xacmlVersion)
    {
        if (inputSamlToken == null || action == null)
        {
            return null;
        }
        var samlToken = ReadSamlToken(inputSamlToken);
        return await GetXacmlRequestFromSamlToken(samlToken, action, xacmlVersion);
    }

    public static async Task<XacmlContextRequest> GetXacmlRequestFromSamlToken(Saml2SecurityToken samlToken, string action, XacmlVersion xacmlVersion)
    {
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var samltokenAuthorizationAttributes = statements.Where(att => att.Name.Contains("xacml") || att.Name.Contains("xspa") || att.Name.Contains("SecurityLevel"));

        var xacmlAttributesList = new List<XacmlContextAttributes>();

        var xacmlActionString = action.ToString();

        XacmlContextRequest request;

        switch (xacmlVersion)
        {
            case XacmlVersion.Version20:

                var subjectAttributes = MapSamlAttributesToXacml20Properties(samltokenAuthorizationAttributes, xacmlActionString);

                // Resource
                var xacmlResourceAttribute = subjectAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id"));

                var xacmlResource = new XacmlContextResource(xacmlResourceAttribute);

                var actionAttribute = new XacmlContextAttribute(
                    new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = xacmlActionString });

                var xacmlAction = new XacmlContextAction(actionAttribute);

                // Subject
                var xacmlSubject = new XacmlContextSubject(subjectAttributes.Where(sa => sa.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)) && sa.AttributeId.OriginalString.Contains("subject")));

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

    private static List<XacmlAttribute> MapSamlAttributesToXacml30Properties(IEnumerable<Saml2Attribute> samltokenAuthorizationAttributes, string action)
    {
        var xacmlAllAttributes = new List<XacmlAttribute>();
        // SAML token values, map to XACML values
        foreach (var samlAttribute in samltokenAuthorizationAttributes)
        {
            foreach (var attributeValue in samlAttribute.Values)
            {
                var attributeValueAsCodedValue = GetSamlAttributeValueAsCodedValue(attributeValue);

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

    private static List<XacmlContextAttribute> MapSamlAttributesToXacml20Properties(IEnumerable<Saml2Attribute> samltokenAuthorizationAttributes, string action)
    {
        var subjectAttributes = new List<XacmlContextAttribute>();

        foreach (var attribute in samltokenAuthorizationAttributes)
        {
            var attributeValue = attribute.Values.FirstOrDefault(); // Never have i ever: seen a SAML-AttributeStatement with more than one Value
            if (attributeValue == null) continue;

            var attributeValueAsCodedValue = GetSamlAttributeValueAsCodedValue(attributeValue);


            var valueExistsAlready = subjectAttributes
                .Any(att => att.AttributeValues
                    .Any(avs =>
                        avs.Value == attributeValueAsCodedValue.Code ||
                        avs.Value == attributeValueAsCodedValue.CodeSystem ||
                        avs.Value == attributeValueAsCodedValue.DisplayName
                    ));

            try
            {

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

        return subjectAttributes;
    }

    private static CodedValue GetSamlAttributeValueAsCodedValue(string attributeValue)
    {
        string code = null;
        string codeSystem = null;
        string displayName = null;

        try
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Regex.Replace(attributeValue, @"\bxsi:\b", ""));
            var attributes = xmlDocument.ChildNodes[0]?.Attributes;

            var type = attributes?.GetNamedItem("type")?.Value;

            code = attributes?.GetNamedItem("code")?.Value ?? attributes?.GetNamedItem("extension")?.Value;
            codeSystem = attributes?.GetNamedItem("codeSystem")?.Value ?? attributes?.GetNamedItem("root")?.Value;
            displayName = attributes?.GetNamedItem("displayName")?.Value;
        }
        catch (Exception)
        {
            var hl7Value = Hl7Object.Parse<CX>(attributeValue);
            if (hl7Value?.AssigningAuthority?.UniversalId == null)
            {
                return new()
                {
                    Code = attributeValue,
                };
            }
        }

        var hl7ObjectValue = Hl7Object.Parse<CX>(attributeValue);
        if (hl7ObjectValue != null && hl7ObjectValue.AssigningAuthority != null)
        {
            code ??= hl7ObjectValue.IdNumber;
            codeSystem ??= hl7ObjectValue.AssigningAuthority.UniversalId;
        }

        return new()
        {
            Code = code,
            CodeSystem = codeSystem,
            DisplayName = displayName
        };
    }

    public static async Task<XacmlContextRequest?> GetXacml30RequestFromSoapEnvelope(string inputSoapEnvelope)
    {
        var samlAssertion = GetSamlTokenFromSoapEnvelope(inputSoapEnvelope);
        var action = MapXacmlActionFromSoapAction(GetActionFromSoapEnvelope(inputSoapEnvelope));
        return await GetXacmlRequestFromSamlToken(samlAssertion, action, XacmlVersion.Version30);
    }

    public static async Task<XacmlContextRequest?> GetXacml20RequestFromSoapEnvelope(string inputSoapEnvelope)
    {
        var samlAssertion = GetSamlTokenFromSoapEnvelope(inputSoapEnvelope);
        var action = MapXacmlActionFromSoapAction(GetActionFromSoapEnvelope(inputSoapEnvelope));
        return await GetXacmlRequestFromSamlToken(samlAssertion, action, XacmlVersion.Version20);
    }


    public static string? GetActionFromSoapEnvelope(string? inputSoapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);
        var soapEnvelopeObject = sxmls.DeserializeSoapMessage<SoapEnvelope>(inputSoapEnvelope);

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
        // Action
        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti18ActionAsync:
            case Constants.Xds.OperationContract.Iti38Action:
            case Constants.Xds.OperationContract.Iti38ActionAsync:
                return Constants.Xacml.Actions.ReadDocumentList;

            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti43ActionAsync:
            case Constants.Xds.OperationContract.Iti39Action:
            case Constants.Xds.OperationContract.Iti39ActionAsync:
                return Constants.Xacml.Actions.ReadDocuments;

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti41ActionAsync:
            case Constants.Xds.OperationContract.Iti42Action:
            case Constants.Xds.OperationContract.Iti42ActionAsync:
                return Constants.Xacml.Actions.Create;

            case Constants.Xds.OperationContract.Iti62Action:
            case Constants.Xds.OperationContract.Iti62ActionAsync:
            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti86ActionAsync:
                return Constants.Xacml.Actions.Delete;

            default:
                return Constants.Xacml.Actions.Unknown;
        }
    }

}
