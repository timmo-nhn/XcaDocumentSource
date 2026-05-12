using System.Security.Cryptography;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.IdentityModel.Tokens.Saml2;
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

    public StatisticsTransformerService(ILogger<StatisticsTransformerService> logger, RegistryWrapper registryWrapper,
        ApplicationConfig appConfig)
    {
        _logger = logger;
        _registryWrapper = registryWrapper;
        _appConfig = appConfig;
    }

    public async Task<UserAccessEntry> TransformToUserAccessEntry(StatisticsRequestAndFields inputFields)
    {
        var userAccessEntry = new UserAccessEntry();

        switch (inputFields.RequestType)
        {
            case RequestAndFieldRequestType.SoapEnvelope:
                userAccessEntry = await GetUserAccessEntryFromSoapEnvelopeBasedRequest(inputFields);
                break;

            case RequestAndFieldRequestType.FhirProvideBundle:
                userAccessEntry = await GetUserAccessEntryFromFhirProvideBundleBasedRequest(inputFields);
                break;

            case RequestAndFieldRequestType.FhirUrlBasedRequest:
                userAccessEntry = await GetUserAccessEntryFromFhirUrlBasedRequest(inputFields);
                break;

            case RequestAndFieldRequestType.Unknown:
            default:
                break;
        }

        return userAccessEntry;
    }

    private async Task<UserAccessEntry> GetUserAccessEntryFromFhirProvideBundleBasedRequest(StatisticsRequestAndFields inputFields)
    {
        var jwt = JwtExtractor.ExtractJwt(inputFields.JwtToken, out _);
        var fhirparser = new FhirJsonDeserializer();

        var fhirBundleRequest = Hl7FhirExtensions.GetResourceFromStream(inputFields.RequestBody) as Bundle;
        
        var fhirBundleResponse = Hl7FhirExtensions.GetResourceFromStream(inputFields.RequestBody) as Bundle;

        if (jwt == null && fhirBundleRequest == null)
            throw new InvalidOperationException("JWT or Fhir Bundle cannot be null.");

        var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(jwt);
        var userAccessEntry = await GetUserAccessEntryFromFhirUrlBasedRequest(inputFields);

        userAccessEntry.Issues = GetIssuesFromFhirResponse(fhirBundleResponse);

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

        return operationOutcome?.Issue.Select(i => $"{i.Severity}: {i.Code} - {i.Diagnostics}").OfType<string>()
            .ToArray();
    }

    private async Task<UserAccessEntry> GetUserAccessEntryFromFhirUrlBasedRequest(StatisticsRequestAndFields inputFields)
    {
        var jwt = JwtExtractor.ExtractJwt(inputFields.JwtToken, out _);

        if (jwt == null) throw new InvalidOperationException("JWT cannot be null.");

        var samlToken = JwtToSamlTransformer.MapJsonWebTokenToSamlToken(jwt);

        var statements = samlToken?.Assertion.Statements.OfType<Saml2AttributeStatement>()
            .SelectMany(statement => statement.Attributes).ToList();

        var subjectOrganization =
            GetSamlAttributeAsCodedValue(statements, "helseid://claims/client/claims/orgnr_parent");
        subjectOrganization?.CodeSystem ??= Constants.Oid.Brreg;

        var subjectChildOrganization =
            GetSamlAttributeAsCodedValue(statements, "urn:oasis:names:tc:xspa:1.0:subject:child-organization");
        subjectChildOrganization?.CodeSystem ??= Constants.Oid.Brreg;

        return new UserAccessEntry()
        {
            SessionId = inputFields.SessionId,
            Issuer = samlToken?.Assertion.Issuer.Value,
            SubjectIdHash = GetSamlAttributeAsHashedString(statements, "helseid://claims/hpr/hpr_number"),
            ResourceIdHash = GetSamlAttributeAsHashedString(statements, "helseid://claims/identity/pid"),
            Action = XacmlExtensions.MapXacmlActionAndFromUrlPath(inputFields.Path, inputFields.Method),
            SubjectOrganization = subjectOrganization,
            SubjectOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.Organization),

            SubjectChildOrganization = subjectChildOrganization,
            SubjectChildOrganizationName =
                GetSamlAttributeAsString(statements, Constants.Saml.Attribute.TrustChildOrgName),

            AccessBasis = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.XuaAcp) ??
                          Constants.Oid.Saml.Acp.NullValue,

            SourceHomeCommunityId = _appConfig.HomeCommunityId,
            SourceRepositoryUniqueId = _appConfig.RepositoryUniqueId,
            SourceHostName = _appConfig.HostName.Split("-xcadocumentsource").FirstOrDefault(),

            DocumentConfidentialityCodes = inputFields.RelatedDocumentEntries
                ?.SelectMany(d => d.ConfidentialityCode ?? []).OfType<CodedValue>()?.ToArray(),
            Endpoint = inputFields.Path,
            ResponseStatusCode = inputFields.StatusCode,
            AccessTime = inputFields.AccessTime,
            ElapsedTimeMillis = inputFields.ElapsedMilliseconds,
        };
    }

    private async Task<UserAccessEntry> GetUserAccessEntryFromSoapEnvelopeBasedRequest(StatisticsRequestAndFields inputFields)
    {
        var sxmls = new SoapXmlSerializer();
        SoapEnvelope? soapEnvelopeRequest = null;
        SoapEnvelope? soapEnvelopeResponse = null;

        if (inputFields.ContentType?.Split(";").FirstOrDefault() == Constants.MimeTypes.MultipartRelated &&
            inputFields.RequestBody?.Length > 0 &&
            inputFields.ResponseBody?.Length > 0)
        {
            soapEnvelopeRequest = await MultipartExtensions.ReadMultipartSoapMessage(inputFields.ContentType, inputFields.RequestBody);
            soapEnvelopeResponse = await MultipartExtensions.ReadMultipartSoapMessage(inputFields.ContentType, inputFields.ResponseBody);
        }
        else
        {
            soapEnvelopeRequest = sxmls.DeserializeXmlString<SoapEnvelope>(inputFields.RequestBody);
            soapEnvelopeResponse = sxmls.DeserializeXmlString<SoapEnvelope>(inputFields.ResponseBody);
        }

        if (soapEnvelopeRequest == null) throw new InvalidOperationException("Soap request envelope cannot be null");

        var samlToken = SamlExtensions.ReadSamlToken(soapEnvelopeRequest.Header?.Security?.Assertion?.OuterXml);

        var statements = samlToken?.Assertion.Statements.OfType<Saml2AttributeStatement>()
            .SelectMany(statement => statement.Attributes).ToList();

        return new UserAccessEntry()
        {
            SessionId = soapEnvelopeRequest.Header?.MessageId,
            Issuer = samlToken?.Assertion.Issuer.Value,
            SubjectIdHash = GetSamlAttributeAsHashedString(statements, Constants.Saml.Attribute.ProviderIdentifier),
            ResourceIdHash = GetSamlAttributeAsHashedString(statements, Constants.Saml.Attribute.ResourceId10,
                Constants.Saml.Attribute.ResourceId20),

            SubjectOrganization = GetSamlAttributeAsCodedValue(statements, Constants.Saml.Attribute.OrganizationId),
            SubjectOrganizationName = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.Organization),

            SubjectChildOrganization =
                GetSamlAttributeAsCodedValue(statements, Constants.Saml.Attribute.ChildOrganization),
            SubjectChildOrganizationName =
                GetSamlAttributeAsString(statements, Constants.Saml.Attribute.TrustChildOrgName),
            AccessBasis = GetSamlAttributeAsString(statements, Constants.Saml.Attribute.XuaAcp) ??
                          Constants.Oid.Saml.Acp.NullValue,

            SourceHomeCommunityId = _appConfig.HomeCommunityId,
            SourceRepositoryUniqueId = _appConfig.RepositoryUniqueId,
            SourceHostName = _appConfig.HostName.Split("-xcadocumentsource").FirstOrDefault(),

            DocumentConfidentialityCodes = GetConfidentialityCodeFromRetrievedDocument(soapEnvelopeRequest),
            Endpoint = inputFields.Path,
            Action = soapEnvelopeRequest.Header?.Action,
            ResponseStatusCode = inputFields.StatusCode,
            AccessTime = inputFields.AccessTime,
            ElapsedTimeMillis = inputFields.ElapsedMilliseconds,
            Issues = GetIssuesFromSoapEnvelope(soapEnvelopeResponse),
        };
    }

    private string[]? GetIssuesFromSoapEnvelope(SoapEnvelope? soapEnvelope)
    {
        RegistryErrorType[] errors =
        [
            .. soapEnvelope?.Body.RetrieveDocumentSetResponse?.RegistryResponse?.RegistryErrorList?.RegistryError ?? [],
            ..soapEnvelope?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError ?? [],
            ..soapEnvelope?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? [],
        ];

        return errors.Select(e => $"{e.ErrorCode}: {e.CodeContext}").OfType<string>().ToArray();
    }

    private static string? GetSamlAttributeAsString(List<Saml2Attribute>? statements, params string[] attributeNames)
    {
        return GetSamlAttributeAsCodedValue(statements, attributeNames)?.Code;
    }

    private static CodedValue? GetSamlAttributeAsCodedValue(List<Saml2Attribute>? statements,
        params string[] attributeNames)
    {
        var subjectOrganization =
            statements?.FirstOrDefault(s => s.Name.IsAnyOf(attributeNames))?.Values.FirstOrDefault();
        return SamlExtensions.GetSamlAttributeValueAsCodedValue(subjectOrganization);
    }

    private static string? GetSamlAttributeAsHashedString(List<Saml2Attribute>? statements,
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