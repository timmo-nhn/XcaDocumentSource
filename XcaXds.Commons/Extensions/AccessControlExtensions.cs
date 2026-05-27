using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using static XcaXds.Commons.Commons.Constants.Xds.AssociationType;

namespace XcaXds.Commons.Extensions;

public static class AccessControlExtensions
{
    public static string MapXacmlActionFromUrlPath(string? urlPath, string? method)
    {
        (string action, string? _) = GetActionAndScopeToUseFromUrlPath(urlPath, method);
        return action;
    }

    public static (string action, string? scopeToUse) GetActionAndScopeToUseFromUrlPath(string? urlPath, string? method)
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