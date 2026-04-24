using Microsoft.IdentityModel.Tokens.Saml2;
using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Extensions;

public class SamlExtensions
{
    public static AppliesTo GetIssuerEnumFromSamlToken(Saml2SecurityToken? samlToken)
    {
        var issuer = samlToken?.Issuer;

        if (!string.IsNullOrWhiteSpace(issuer))
        {
            if (issuer.Contains("helseid-xdssaml"))
            {
                return AppliesTo.HelseId;
            }
            if (issuer.Contains("helsenorge"))
            {
                return AppliesTo.Helsenorge;
            }
            if (IsMachineToMachineToken(samlToken))
            {
                return AppliesTo.Machine;
            }
        }
        return AppliesTo.Unknown;
    }

    private static bool IsMachineToMachineToken(Saml2SecurityToken? samlToken)
    {
        var statements = samlToken?.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var samltokenAuthorizationAttributes = statements?
            .Where(att =>
                 att.Name.Contains("xacml") ||
                 att.Name.Contains("xspa") ||
                 att.Name.Contains("provider-identifier") ||
                 att.Name.Contains("trust-framework"))
            .ToList();

        return samltokenAuthorizationAttributes?.Count == 0;
    }

    public static AppliesTo GetIssuerEnumFromSamlToken(string issuer)
    {
        if (!string.IsNullOrWhiteSpace(issuer))
        {
            if (issuer.Contains("helseid"))
            {
                return AppliesTo.HelseId;
            }
            if (issuer.Contains("helsenorge"))
            {
                return AppliesTo.Helsenorge;
            }
        }
        return AppliesTo.Unknown;
    }

    public static Saml2SecurityToken? ReadSamlToken(string? inputSamlToken)
    {
        if (inputSamlToken == null) return null;

        inputSamlToken = Regex.Unescape(inputSamlToken);

        try
        {
            var handler = new Saml2SecurityTokenHandler();
            return handler.ReadSaml2Token(inputSamlToken);
        }
        catch
        {
            return null;
        }
    }

    public static CodedValue? GetSamlAttributeValueAsCodedValue(string? attributeValue)
    {
        if (attributeValue == null) return null;

        string? code = null;
        string? codeSystem = null;
        string? displayName = null;

        try
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Regex.Replace(attributeValue, @"\b:?xsi:?\b", ""));
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
            CodeSystem = codeSystem?.Replace("&ISO", "").Replace("&amp;ISO", "").Replace("&amp;amp;ISO",""),
            DisplayName = displayName
        };
    }

    public static CX? GetSamlAttributeValueAsCx(string? subjectId)
    {
        var codedValue = GetSamlAttributeValueAsCodedValue(subjectId);

        if (codedValue == null) return null;

        return new(codedValue.Code, codedValue.CodeSystem);
    }
}
