using Abc.Xacml.Context;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Extensions;

public static class XacmlExtensions
{
    public static List<XacmlContextAttribute>? GetAllXacmlContextAttributes(this XacmlContextRequest xacmlRequest)
    {
        return xacmlRequest.Subjects
            .SelectMany(x => x.Attributes)
            .Concat(xacmlRequest.Resources
            .SelectMany(x => x.Attributes))
            .Concat(xacmlRequest.Environment.Attributes)
            .Concat(xacmlRequest.Action.Attributes).ToList();
    }

    public static List<XacmlContextAttribute>? GetXacmlContextAttributesById(this XacmlContextRequest xacmlRequest, string id)
    {
        var allAttributes = xacmlRequest.GetAllXacmlContextAttributes();

        return allAttributes?.Where(att => att.AttributeId.AbsoluteUri.Contains(id)).ToList();
    }

    public static List<XacmlContextAttribute>? GetXacmlContextAttributesById(this List<XacmlContextAttribute>? xacmlAttributes, string id)
    {
        return xacmlAttributes?.Where(att => att.AttributeId.AbsoluteUri.Contains(id)).ToList();
    }

    public static CodedValue? GetXacmlAttributeAsCodedValue(this XacmlContextRequest xacmlRequest, string attributeValue)
    {
        var filteredAttributes = xacmlRequest.GetXacmlContextAttributesById(attributeValue)?.Distinct()?
            .ToDictionary(k => k.AttributeId.AbsoluteUri, v => v.AttributeValues.FirstOrDefault()?.Value);

        if (filteredAttributes == null) return null;

        return new CodedValue()
        {
            Code = filteredAttributes.GetValueOrDefault(attributeValue + ":code"),
            CodeSystem = filteredAttributes.GetValueOrDefault(attributeValue + ":codeSystem"),
            DisplayName = filteredAttributes.GetValueOrDefault(attributeValue + ":displayName"),
        };
    }

    public static CodedValue? GetXacmlAttributeValuesAsCodedValue(this List<XacmlContextAttribute>? xacmlAttributes, string attributeValue)
    {
        var filteredAttributes = xacmlAttributes.GetXacmlContextAttributesById(attributeValue)?.Distinct()?
            .ToDictionary(k => k.AttributeId.AbsoluteUri, v => v.AttributeValues.FirstOrDefault()?.Value);

        if (filteredAttributes == null || filteredAttributes.Count == 0) return null;

        return new CodedValue()
        {
            Code = filteredAttributes.GetValueOrDefault(attributeValue + ":code"),
            CodeSystem = filteredAttributes.GetValueOrDefault(attributeValue + ":codeSystem"),
            DisplayName = filteredAttributes.GetValueOrDefault(attributeValue + ":displayName"),
        };
    }

    public static List<string?>? GetXacmlAttributeValuesAsString(this List<XacmlContextAttribute>? xacmlAttributes, string id)
    {
        return xacmlAttributes?.Where(att => att.AttributeId.AbsoluteUri.Contains(id)).Select(att => att.AttributeValues.FirstOrDefault()?.Value).ToList();
    }
}
