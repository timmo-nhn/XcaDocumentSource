using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Extensions;

public class SamlExtensions
{
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
            CodeSystem = codeSystem,
            DisplayName = displayName
        };
    }
}
