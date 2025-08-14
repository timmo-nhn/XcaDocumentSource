using System.Text.RegularExpressions;
using System.Xml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Services;

public class PolicyAuthorizationService
{
    public PolicyAuthorizationService()
    {

    }

    public async Task<XacmlContextRequest?> GetXacmlRequestFromJsonWebToken(string inputJson)
    {
        XacmlContextRequest contextRequest = null;
        return contextRequest;
    }


    public async Task<XacmlContextRequest> GetXacmlRequestFromSamlToken(string inputSamlToken, string action, Constants.Xacml.XacmlVersion xacmlVersion)
    {
        if (inputSamlToken == null || action == null)
        {
            return null;
        }

        var handler = new Saml2SecurityTokenHandler();
        var samlToken = handler.ReadSaml2Token(inputSamlToken);
        return await GetXacmlRequestFromSamlToken(samlToken, action, xacmlVersion);
    }

    public async Task<XacmlContextRequest> GetXacmlRequestFromSamlToken(Saml2SecurityToken samlToken, string action, Constants.Xacml.XacmlVersion xacmlVersion)
    {
        var authStatement = samlToken.Assertion.Statements.OfType<Saml2AuthenticationStatement>().FirstOrDefault();
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var samltokenAuthorizationAttributes = statements.Where(att => att.Name.Contains("xacml"));

        var xacmlAttributesList = new List<XacmlContextAttributes>();

        
        var subjectAttributes = MapSamlAttributesToXacml20Properties(samltokenAuthorizationAttributes, action);


        var xacmlAllAttributes = MapSamlAttributesToXacml30Properties(samltokenAuthorizationAttributes, action);



        XacmlContextRequest request;

        if (xacmlVersion == Constants.Xacml.XacmlVersion.Version30)
        {

            var xacmlActionAttribute = new XacmlAttribute(new Uri(Constants.Xacml.Attribute.ActionId), false);
            var xacmlActionAttributeValue = new XacmlAttributeValue(new Uri(Constants.Xacml.DataType.String), action);
            xacmlActionAttribute.AttributeValues.Add(xacmlActionAttributeValue);
            xacmlAllAttributes.Add(xacmlActionAttribute);

            var xacmlSubjectContextAttributes = new XacmlContextAttributes(
                new Uri(Constants.Xacml.Category.Subject),
                xacmlAllAttributes.Where(xatt => xatt.AttributeId.AbsolutePath.Contains("subject")));

            var xacmlResourceContextAttributes = new XacmlContextAttributes(
                new Uri(Constants.Xacml.Category.Resource),
                xacmlAllAttributes.Where(xatt => xatt.AttributeId.AbsolutePath.Contains("resource-id")));

            var xacmlActionContextAttributes = new XacmlContextAttributes(
                new Uri(Constants.Xacml.Category.Action),
                xacmlAllAttributes.Where(xatt => xatt.AttributeId.AbsolutePath.Contains("action-id")));

            xacmlAttributesList.Add(xacmlSubjectContextAttributes);
            xacmlAttributesList.Add(xacmlResourceContextAttributes);
            xacmlAttributesList.Add(xacmlActionContextAttributes);

            request = new XacmlContextRequest(false, false, xacmlAttributesList);
            request.ReturnPolicyIdList = false;
            request.CombinedDecision = false;

            return request;
        }

        if (xacmlVersion == Constants.Xacml.XacmlVersion.Version20)
        {

            var actionAttribute = new XacmlContextAttribute(
                new Uri(Constants.Xacml.Attribute.ActionId), new Uri(Constants.Xacml.DataType.String), new XacmlContextAttributeValue() { Value = action });

            var xacmlAction = new XacmlContextAction(actionAttribute);

            // Subject
            var xacmlSubject = new XacmlContextSubject(subjectAttributes.Where(sa => sa.AttributeId.OriginalString.Contains("resource-id") == false));

            // Environment
            var xacmlEnvironment = new XacmlContextEnvironment();

            var request = new XacmlContextRequest(xacmlResource, xacmlAction, xacmlSubject);

            request.ReturnPolicyIdList = false;
            request.CombinedDecision = false;

            return request;
        }

        return null;
    }

    private List<XacmlAttribute> MapSamlAttributesToXacml30Properties(IEnumerable<Saml2Attribute> samltokenAuthorizationAttributes, string action)
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

    private List<XacmlContextAttribute> MapSamlAttributesToXacml20Properties(IEnumerable<Saml2Attribute> samltokenAuthorizationAttributes, string action)
    {
        var subjectAttributes = new List<XacmlContextAttribute>();

        foreach (var attribute in samltokenAuthorizationAttributes)
        {
            foreach (var attributeValue in attribute.Values)
            {
                string finalAttributeValue = null;

                var attributeValueAsCodedValue = GetSamlAttributeValueAsCodedValue(attributeValue);

                // If its structured codedvalue format or just plain text
                if (attributeValueAsCodedValue != null)
                {
                    if (attributeValueAsCodedValue.Code != null)
                    {
                        subjectAttributes.Add(
                            new XacmlContextAttribute(
                                new Uri(attribute.Name + ":code"),
                                new Uri(Constants.Xacml.DataType.String),
                                new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.Code }));
                    }

                    if (attributeValueAsCodedValue.CodeSystem != null)
                    {
                        subjectAttributes.Add(
                            new XacmlContextAttribute(
                                new Uri(attribute.Name + ":codeSystem"),
                                new Uri(Constants.Xacml.DataType.String),
                                new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.CodeSystem }));

                    }

                    if (attributeValueAsCodedValue.DisplayName != null)
                    {
                        subjectAttributes.Add(
                            new XacmlContextAttribute(
                                new Uri(attribute.Name + ":displayName"),
                                new Uri(Constants.Xacml.DataType.String),
                                new XacmlContextAttributeValue() { Value = attributeValueAsCodedValue.DisplayName }));
                    }
                }
                else
                {
                    finalAttributeValue = attributeValue;
                    subjectAttributes.Add(
                        new XacmlContextAttribute(
                            new Uri(attribute.Name),
                            new Uri(Constants.Xacml.DataType.String),
                            new XacmlContextAttributeValue() { Value = finalAttributeValue }
                    ));
                }
            }
        }

        return subjectAttributes;
    }

    private CodedValue? GetSamlAttributeValueAsCodedValue(string attributeValue)
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

            code = attributes?.GetNamedItem("code")?.Value;
            codeSystem = attributes?.GetNamedItem("codeSystem")?.Value?.NoUrn();
            displayName = attributes?.GetNamedItem("displayName")?.Value;
        }
        catch (Exception)
        {
            var hl7Value = Hl7Object.Parse<CX>(attributeValue);
            if (hl7Value?.AssigningAuthority?.UniversalId == null)
            {
                // If its codeable from neither XML or HL7, then treat it as plain text.
                return null;
            }
        }

        var hl7ObjectValue = Hl7Object.Parse<CX>(attributeValue);
        if (hl7ObjectValue != null)
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

    public async Task<XacmlContextRequest?> GetXacml30RequestFromSoapEnvelope(string inputSoapEnvelope)
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var soapEnvelopeXmlDocument = new XmlDocument();
        try
        {
            soapEnvelopeXmlDocument.LoadXml(inputSoapEnvelope);
        }
        catch (Exception)
        {
            return null;
        }

        var soapEnvelopeObject = await sxmls.DeserializeSoapMessageAsync<SoapEnvelope>(inputSoapEnvelope);

        var assertion = soapEnvelopeXmlDocument.GetElementsByTagName("saml:Assertion");

        if (assertion.Count == 0)
        {
            throw new Exception("No SAML Assertion found in the SOAP envelope.");
        }

        var samlAssertion = assertion[0]?.OuterXml;

        return await GetXacmlRequestFromSamlToken(samlAssertion, soapEnvelopeObject.Header.Action, Constants.Xacml.XacmlVersion.Version20);
    }

    private static string MapXacmlActionFromSoapAction(string action)
    {
        // Action
        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti18ActionAsync:
            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti43ActionAsync:
            case Constants.Xds.OperationContract.Iti38Action:
            case Constants.Xds.OperationContract.Iti38ActionAsync:
            case Constants.Xds.OperationContract.Iti39Action:
            case Constants.Xds.OperationContract.Iti39ActionAsync:
                return "read";

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti41ActionAsync:
            case Constants.Xds.OperationContract.Iti42Action:
            case Constants.Xds.OperationContract.Iti42ActionAsync:
                return "write";

            case Constants.Xds.OperationContract.Iti62Action:
            case Constants.Xds.OperationContract.Iti62ActionAsync:
            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti86ActionAsync:
                return "delete";

            default:
                return null;
        }
    }
}
