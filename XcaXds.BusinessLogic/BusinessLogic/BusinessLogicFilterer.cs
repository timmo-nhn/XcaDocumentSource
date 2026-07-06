using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.BusinessLogic.Models.Custom.BusinessLogic;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;

namespace XcaXds.BusinessLogic.BusinessLogic;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs. Allows for partial filtering of the document list
/// </summary>
public class DocumentListFiltererService
{
    private readonly ILogger<DocumentListFiltererService> _logger;
    private readonly BusinessLogicFiltersRegistry _businessLogicFiltersRegistry;
    private readonly BusinessLogicMapperService _businessLogicMapperService;
    private readonly DocumentObfuscationService _documentObfuscationService;

    public DocumentListFiltererService(
        ILogger<DocumentListFiltererService> logger,
        BusinessLogicFiltersRegistry businessLogicFiltersRegistry,
        BusinessLogicMapperService businessLogicMapperService,
        DocumentObfuscationService documentObfuscationService)
    {
        _logger = logger;
        _businessLogicFiltersRegistry = businessLogicFiltersRegistry;
        _businessLogicMapperService = businessLogicMapperService;
        _documentObfuscationService = documentObfuscationService;

        BusinessLogicRules = _businessLogicFiltersRegistry.AllBusinessRules;
    }

    public Dictionary<string, BusinessRule<IdentifiableType>> BusinessLogicRules = default!;


    public void AddRule(string key, BusinessRule<IdentifiableType> rule)
    {
        BusinessLogicRules.Add(key, rule);
    }

    public void RemoveRule(string key)
    {
        BusinessLogicRules.Remove(key);
    }

    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _compiled = new();

    private static Func<BusinessLogicParameters, bool> CompileCached(Expression<Func<BusinessLogicParameters, bool>> expr)
    {
        return (Func<BusinessLogicParameters, bool>)_compiled.GetOrAdd(expr, e => e.Compile());
    }

    public IEnumerable<IdentifiableType> FilterRegistryObjectListBasedOnBusinessLogic(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters? businessLogic, out Dictionary<string, int> appliedRules)
    {
        appliedRules = new Dictionary<string, int>();

        if (businessLogic == null) return registryObjects;

        var current = registryObjects;

        var rulesApplied = new List<BusinessLogicResult<IdentifiableType>>();

        var resultCounts = new Dictionary<string, int>();

        foreach (var businessRule in BusinessLogicRules)
        {
            var result = ExecuteRule(businessRule, current ?? [], businessLogic);

            if (result.RuleApplied)
            {
                rulesApplied.Add(result);
                current = result.RegistryObjects;
                resultCounts.Add(result.Name ?? "Unnamed business rule", (current != null && current.TryGetNonEnumeratedCount(out var count)) ? count : current?.Count() ?? 0);
            }
        }

        // The Business logic rules should cover every normal scenario...
        // If no rules were applicable, we've hit an edge case, so filter out everything!
        if (rulesApplied.Count == 0)
        {
            current = [];
        }

        appliedRules = resultCounts;
        return current ?? [];
    }

    public SoapEnvelope FilterAdhocQueryResponseBasedOnBusinessLogic(SoapEnvelope requestEnvelope, SoapEnvelope soapEnvelope, AbacRequest? abacRequest, out Dictionary<string, int>? filterResults)
    {
        filterResults = null;

        // Apply business-logic filtering
        var requestType = requestEnvelope.Body.AdhocQueryRequest?.AdhocQuery.Id;

        if (requestType == Constants.Xds.StoredQueries.FindDocuments)
        {
            List<IdentifiableType> enumeratedEntriesResult = soapEnvelope.Body.AdhocQueryResponse?.RegistryObjectList?.ToList() ?? [];

            _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Applying business logic, current XDSEntries count: {enumeratedEntriesResult.Count}");

            var businessLogic = _businessLogicMapperService.MapFromAbacRequestToBusinessLogic(abacRequest);

            _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Business logic: {JsonSerializer.Serialize(businessLogic, Constants.JsonDefaultOptions.DefaultSettings)}");

            enumeratedEntriesResult = FilterRegistryObjectListBasedOnBusinessLogic(enumeratedEntriesResult, businessLogic, out filterResults).ToList();

            var gobb = JsonSerializer.Serialize(businessLogic);

            if (filterResults.Count > 0)
            {
                _logger.LogInformation($"{soapEnvelope.Header.MessageId} - Business logic applied: {JsonSerializer.Serialize(filterResults)}");
            }
            else
            {
                _logger.LogInformation($"{soapEnvelope.Header.MessageId} - No business logic applied, XDSEntries count: {enumeratedEntriesResult.Count}");
            }

            enumeratedEntriesResult = _documentObfuscationService.ObfuscateRestrictedDocumentEntries(enumeratedEntriesResult, businessLogic, out var obfuscateCount);
            var sxmls = new SoapXmlSerializer();
            var content = sxmls.SerializeSoapMessageToXmlString(enumeratedEntriesResult).Content;
            _logger.LogInformation($"{soapEnvelope.Header.MessageId} - {obfuscateCount} XDSEntries obfuscated");

            soapEnvelope.Body.AdhocQueryResponse?.RegistryObjectList = [.. enumeratedEntriesResult];
        }

        return soapEnvelope;
    }

    public static BusinessLogicResult<T> ExecuteRule<T>(KeyValuePair<string, BusinessRule<T>> rule, IEnumerable<T> objects, BusinessLogicParameters logic)
    {
        var condition = CompileCached(rule.Value.Condition!);

        if (condition(logic))
        {
            var filtered = rule.Value.Filter!.Compile()(objects);
            return new(true, filtered, rule.Key);
        }

        return new(false, objects, rule.Key);
    }
}