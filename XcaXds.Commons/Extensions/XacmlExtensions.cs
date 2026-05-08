using Abc.Xacml.Context;
using System.Net;
using System.Text.RegularExpressions;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using static XcaXds.Commons.Commons.Constants.Xds.AssociationType;

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
            .ToDictionary(k => k.AttributeId.AbsoluteUri, v => v.AttributeValues.FirstOrDefault()?.Value != null ? WebUtility.HtmlDecode(Regex.Unescape(v.AttributeValues.First().Value)) : null);

        if (filteredAttributes == null || filteredAttributes.Count == 0) return null;

        return new CodedValue()
        {
            Code = filteredAttributes.GetValueOrDefault(attributeValue + ":code") ?? filteredAttributes.GetValueOrDefault(attributeValue),
            CodeSystem = filteredAttributes.GetValueOrDefault(attributeValue + ":codeSystem"),
            DisplayName = filteredAttributes.GetValueOrDefault(attributeValue + ":displayName"),
        };
    }

    public static List<string>? GetXacmlAttributeValuesAsString(this List<XacmlContextAttribute>? xacmlAttributes, string id)
    {
        return xacmlAttributes?.Where(att => att.AttributeId.AbsoluteUri.Contains(id)).Select(att => att.AttributeValues.FirstOrDefault()?.Value).OfType<string>().ToList();
    }

    public static string MapXacmlActionAndFromUrlPath(string? urlPath, string? method)
    {
        (string action, string? _ ) =  MapXacmlActionAndScopeToUseFromUrlPath(urlPath, method);
        return action;
    }

    public static (string action, string? scopeToUse) MapXacmlActionAndScopeToUseFromUrlPath(string? urlPath, string? method)
    {
        if (urlPath?.Equals("/R4/fhir/Bundle", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.Create, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments);

        if (urlPath?.StartsWith("/R4/fhir/Bundle", StringComparison.InvariantCultureIgnoreCase) == true && method == "PATCH")
            return (Constants.Xacml.Actions.Update, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments);

        if (urlPath?.Equals("/R4/fhir/mhd/document", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.ReadDocuments, null);

        if (urlPath?.Equals("/R4/fhir/DocumentReference/_search", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.ReadDocumentList, null);

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "GET")
            return (Constants.Xacml.Actions.ReadDocumentList, null);

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "PATCH")
            return (Constants.Xacml.Actions.Update, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments);

        if (urlPath?.StartsWith("/R4/fhir/DocumentReference", StringComparison.InvariantCultureIgnoreCase) == true && method == "DELETE")
            return (Constants.Xacml.Actions.Delete, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeDeleteDocument);

        if (urlPath?.StartsWith("/R4/fhir/", StringComparison.InvariantCultureIgnoreCase) == true && urlPath?.EndsWith("$validate", StringComparison.InvariantCultureIgnoreCase) == true && method == "POST")
            return (Constants.Xacml.Actions.Execute, Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments);

        return (Constants.Xacml.Actions.Create, null);
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
