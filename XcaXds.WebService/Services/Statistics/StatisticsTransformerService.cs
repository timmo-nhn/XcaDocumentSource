using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;

namespace XcaXds.WebService.Services.Statistics;

public class StatisticsTransformerService
{
    private readonly ILogger<StatisticsTransformerService> _logger;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ApplicationConfig _appConfig;

    public StatisticsTransformerService(ILogger<StatisticsTransformerService> logger, RegistryWrapper registryWrapper, ApplicationConfig appConfig)
    {
        _logger = logger;
        _registryWrapper = registryWrapper;
        _appConfig = appConfig;
    }

    public async Task<UserAccessEntry> TransformToUserAccessEntry(StatisticsRequestAndFields inputFields)
    {
        return inputFields.RequestType switch
        {
            RequestAndFieldRequestType.SoapEnvelope => await GetUserAccessEntryFromSoapEnvelopeBasedRequest(inputFields),
            RequestAndFieldRequestType.FhirProvideBundle => await GetUserAccessEntryFromFhirProvideBundleBasedRequest(inputFields),
            RequestAndFieldRequestType.FhirUrlBasedRequest => await GetUserAccessEntryFromFhirUrlBasedRequest(inputFields),
            _ => throw new ArgumentOutOfRangeException("Unknown RequestType for RequestandFields: " + JsonSerializer.Serialize(inputFields, Constants.JsonDefaultOptions.DefaultSettings)),
        };
    }

    private async Task<UserAccessEntry> GetUserAccessEntryFromFhirProvideBundleBasedRequest(StatisticsRequestAndFields inputFields)
    {
        var jwt = JwtExtractor.ExtractJwt(inputFields.JwtToken, out _);

        var fhirBundleRequest = Hl7FhirExtensions.GetResourceFromStream(inputFields.RequestBody) as Bundle;
        var fhirBundleResponse = Hl7FhirExtensions.GetResourceFromStream(inputFields.ResponseBody) as Bundle;

        var uploadedEntries = fhirBundleRequest?.Entry.Select(res => res.Resource).OfType<Binary>().ToList();

        if (jwt == null && fhirBundleRequest == null)
            throw new InvalidOperationException("JWT or Fhir Bundle cannot be null.");

        var userAccessEntry = await GetUserAccessEntryFromFhirUrlBasedRequest(inputFields, fhirBundleResponse);
        userAccessEntry.UploadedEntries = uploadedEntries?.Count;

        return userAccessEntry;
    }

    private string[]? GetIssuesFromFhirResponse(Resource? fhirResource)
    {
        var responseBundle = fhirResource as Bundle;
        var responseOperationOutcome = fhirResource as OperationOutcome;

        var operationOutcome = responseBundle?.Entry
            .Select(e => e.Resource)
            .OfType<OperationOutcome>()
            .FirstOrDefault() ?? responseOperationOutcome;

        return operationOutcome?.Issue.Select(i => $"{i.Severity}: {i.Code} - {i.Diagnostics}").ToArray();
    }

    private async Task<UserAccessEntry> GetUserAccessEntryFromFhirUrlBasedRequest(StatisticsRequestAndFields inputFields, Resource? fhirBundleResponse = null)
    {
        var jwt = JwtExtractor.ExtractJwt(inputFields.JwtToken, out _);

        if (jwt == null) throw new InvalidOperationException("JWT cannot be null.");

        var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(jwt);

        fhirBundleResponse ??= Hl7FhirExtensions.GetResourceFromStream(inputFields.ResponseBody);

        var statements = samlToken?.GetAllStatements();

        var subjectOrganization = GetSamlAttributeAsCodedValue(statements, "helseid://claims/client/claims/orgnr_parent");
        subjectOrganization?.CodeSystem ??= Constants.Oid.Brreg;

        var subjectChildOrganization = GetSamlAttributeAsCodedValue(statements, "urn:oasis:names:tc:xspa:1.0:subject:child-organization");
        subjectChildOrganization?.CodeSystem ??= Constants.Oid.Brreg;

        return new UserAccessEntry()
        {
            SessionId = inputFields.SessionId,
            Issuer = samlToken?.Assertion.Issuer.Value,
            SubjectIdHash = GetSamlAttributeAsHashedString(statements, "helseid://claims/hpr/hpr_number"),
            ResourceIdHash = GetSamlAttributeAsHashedString(statements, "helseid://claims/identity/pid"),
            Action = AccessControlExtensions.MapXacmlActionFromUrlPath(inputFields.Path, inputFields.Method),
            SubjectOrganization = subjectOrganization,
            SubjectOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.Organization),

            SubjectChildOrganization = subjectChildOrganization,
            SubjectChildOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.TrustChildOrgName),

            AccessBasis = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.XuaAcp) ?? Constants.Oid.Saml.Acp.NullValue,

            SourceHomeCommunityId = _appConfig.HomeCommunityId,
            SourceRepositoryUniqueId = _appConfig.RepositoryUniqueId,
            SourceHostName = _appConfig.HostName.Split("-xcadocumentsource").FirstOrDefault(),

            DocumentConfidentialityCodes = inputFields.RelatedDocumentEntries?.SelectMany(d => d.ConfidentialityCode ?? []).ToArray(),
            Endpoint = inputFields.Path,
            Success = GetSuccessTypeFromFhirResponse(fhirBundleResponse),
            ResponseStatusCode = inputFields.StatusCode,
            AccessTime = inputFields.AccessTime,
            ElapsedTimeMillis = inputFields.ElapsedMilliseconds,
            Issues = GetIssuesFromFhirResponse(fhirBundleResponse)
        };
    }

    private static SuccessType GetSuccessTypeFromFhirResponse(Resource? resourceResponse)
    {
        List<OperationOutcome> bundleOutcomes = resourceResponse is Bundle bundle
            ? bundle.Entry
                .Select(ent => ent.Resource)
                .OfType<OperationOutcome>()
                .ToList()
            : [];

        List<OperationOutcome> directOutcome = resourceResponse is OperationOutcome opOutcome
                ? [opOutcome]
                : [];

        var operationOutcomes = bundleOutcomes.Concat(directOutcome);

        var enumerable = operationOutcomes as OperationOutcome[] ?? operationOutcomes.ToArray();
        var outcomeTuple = (
            HasErrorsOrFatals: enumerable?.Sum(oo => oo.Errors + oo.Fatals) > 0,
            HasWarnings: enumerable?.Sum(oo => oo.Warnings) > 0
        );

        return outcomeTuple switch
        {
            (false, false) => SuccessType.Success,
            (true, _) => SuccessType.Failure,
            (false, true) => SuccessType.SuccessWithErrors
        };
    }

    private async Task<UserAccessEntry> GetUserAccessEntryFromSoapEnvelopeBasedRequest(StatisticsRequestAndFields inputFields)
    {
        var sxmls = new SoapXmlSerializer();
        SoapEnvelope? soapEnvelopeRequest = null;
        SoapEnvelope? soapEnvelopeResponse = null;

        var requestIsMultipart = inputFields.RequestContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated && inputFields.RequestBody?.Length > 0;
        var responseIsMultipart = inputFields.ResponseContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated && inputFields.ResponseBody?.Length > 0;

        if (requestIsMultipart)
        {
            soapEnvelopeRequest = await MultipartExtensions.ReadMultipartSoapMessage(inputFields.RequestBody, inputFields.RequestContentType);
            soapEnvelopeResponse = await MultipartExtensions.ReadMultipartSoapMessage(inputFields.ResponseBody, inputFields.ResponseContentType);
        }
        else
        {
            if (inputFields.RequestBody?.Length > 0)
                soapEnvelopeRequest = sxmls.DeserializeXmlString<SoapEnvelope>(inputFields.RequestBody);

            if (inputFields.ResponseBody?.Length > 0)
                soapEnvelopeResponse = sxmls.DeserializeXmlString<SoapEnvelope>(inputFields.ResponseBody);
        }
        if (soapEnvelopeRequest == null) throw new InvalidOperationException("Soap request envelope cannot be null");

        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelopeRequest.Header?.Security?.Assertion?.OuterXml);
        var uploadedEntries = soapEnvelopeRequest.Body.ProvideAndRegisterDocumentSetRequest?.Document;

        var statements = samlToken?.GetAllStatements().ToList();

        return new UserAccessEntry()
        {
            SessionId = soapEnvelopeRequest.Header?.MessageId,
            Issuer = samlToken?.Assertion.Issuer.Value,
            SubjectIdHash = GetSamlAttributeAsHashedString(statements, Constants.Saml.Attribute.ProviderIdentifier),
            ResourceIdHash = GetSamlAttributeAsHashedString(statements, Constants.Saml.Attribute.ResourceId10,
                Constants.Saml.Attribute.ResourceId20),

            SubjectOrganization = GetSamlAttributeAsCodedValue(statements, Constants.Saml.Attribute.OrganizationId),
            SubjectOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.Organization),

            SubjectChildOrganization = GetSamlAttributeAsCodedValue(statements, Constants.Saml.Attribute.ChildOrganization),
            SubjectChildOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.TrustChildOrgName),
            AccessBasis = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.XuaAcp) ??
                          Constants.Oid.Saml.Acp.NullValue,
            UploadedEntries = uploadedEntries?.Length,
            SourceHomeCommunityId = _appConfig.HomeCommunityId,
            SourceRepositoryUniqueId = _appConfig.RepositoryUniqueId,
            SourceHostName = _appConfig.HostName.Split("-xcadocumentsource").FirstOrDefault(),
            Success = GetSuccessTypeFromRegistryError(SoapExtensions.RegistryErrorsFromSoapEnvelope(soapEnvelopeResponse), soapEnvelopeResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse),
            DocumentConfidentialityCodes = GetConfidentialityCodeFromRetrievedDocument(soapEnvelopeRequest),
            Endpoint = inputFields.Path,
            Action = soapEnvelopeRequest.Header?.Action,
            ResponseStatusCode = inputFields.StatusCode,
            AccessTime = inputFields.AccessTime,
            ElapsedTimeMillis = inputFields.ElapsedMilliseconds,
            Issues = GetFormattedIssuesFromSoapEnvelope(soapEnvelopeResponse),
        };
    }

    private static SuccessType GetSuccessTypeFromRegistryError(RegistryErrorList? registryErrorsFromSoapEnvelope, DocumentResponseType[]? documentResponse)
    {
        var anyErrors = registryErrorsFromSoapEnvelope?.RegistryError.Any(err => err.Severity == Constants.Xds.ErrorSeverity.Error) ?? false;
        var anyWarnings = registryErrorsFromSoapEnvelope?.RegistryError.Any(err => err.Severity == Constants.Xds.ErrorSeverity.Warning) ?? false;
        var anyDocuments = documentResponse?.Length > 0;

        return (anyErrors, anyWarnings, anyDocuments) switch
        {
            (false, true, true) => SuccessType.SuccessWithErrors,
            (true, _, false) => SuccessType.Failure,
            _ => SuccessType.Success
        };
    }

    private static string[]? GetFormattedIssuesFromSoapEnvelope(SoapEnvelope? soapEnvelope)
    {
        var issues = SoapExtensions.RegistryErrorsFromSoapEnvelope(soapEnvelope);

        return issues?.RegistryError.Select(e => $"{e.ErrorCode}: {e.CodeContext}").ToArray();
    }

    private static string? GetSamlAttributeAsString(IEnumerable<Saml2Attribute>? statements, params string[] attributeNames)
    {
        return GetSamlAttributeAsCodedValue(statements, attributeNames)?.Code;
    }

    private static CodedValue? GetSamlAttributeAsCodedValue(IEnumerable<Saml2Attribute>? statements,
        params string[] attributeNames)
    {
        var subjectOrganization =
            statements?.FirstOrDefault(s => s.Name.IsAnyOf(attributeNames))?.Values.FirstOrDefault();
        return SamlExtensions.GetSamlAttributeValueAsCodedValue(subjectOrganization);
    }

    private static string? GetSamlAttributeAsHashedString(IEnumerable<Saml2Attribute>? statements,
        params string[] attributeNames)
    {
        var samlAttributeCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements
            ?.FirstOrDefault(s => s.Name.IsAnyOf(attributeNames))?.Values.FirstOrDefault());
        var samlStatement = (samlAttributeCoded?.Code + "^" + samlAttributeCoded?.CodeSystem).Trim('^');

        return string.IsNullOrWhiteSpace(samlStatement)
            ? "Unknown"
            : Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(samlStatement)));
    }

    private CodedValue[]? GetConfidentialityCodeFromProvidedBundle(Bundle fhirBundle)
    {
        var documentReferences = fhirBundle.Entry
            .Select(e => e.Resource)
            .OfType<DocumentReference>()
            .ToList();

        return documentReferences
            .SelectMany(dr => dr.SecurityLabel)
            .SelectMany(sl => sl.Coding)
            .Select(cd => new CodedValue(cd.Code, cd.System, cd.Display))
            .ToArray();
    }

    private CodedValue[]? GetConfidentialityCodeFromRetrievedDocument(SoapEnvelope soapEnvelope)
    {
        var documentRequest = soapEnvelope.Body.RetrieveDocumentSetRequest?.DocumentRequest?.FirstOrDefault();

        if (documentRequest == null)
        {
            return null;
        }

        var registryObject = _registryWrapper.GetSingleRegistryObjectAsDto(documentRequest.DocumentUniqueId);
        return (registryObject as DocumentEntryDto)?.ConfidentialityCode?.ToArray();
    }
}