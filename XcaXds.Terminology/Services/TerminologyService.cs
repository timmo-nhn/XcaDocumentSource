using Microsoft.Extensions.Logging;
using System.Globalization;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;

namespace XcaXds.Terminology.Services;

public class TerminologyService
{
    private readonly ILogger<TerminologyService> _logger;

    private Dictionary<string, ComprehensiveCodeSystem[]> CodeSystems { get; set; } = [];

    public TerminologyService(ILogger<TerminologyService> logger)
    {
        _logger = logger;
    }

    public void AddCodeSystem(string name, ComprehensiveCodeSystem[] codeSystems)
    {
        _logger.LogInformation($"Adding code system {name} with {codeSystems.Length} entries...");
        CodeSystems.Add(name, codeSystems);
        _logger.LogInformation($"Added code system {name} with {codeSystems.Length} entries");
    }

    public ComprehensiveCodeSystem[] GetCodeSystemByKey(string name)
    {
        return CodeSystems[name];
    }

    public ComprehensiveCodeSystem[] GetCodeSystemBySystem(string system)
    {
        return CodeSystems.Values.SelectMany(cs => cs).Where(cs => cs.SystemOid == system || cs.SystemUrl == system).ToArray();
    }

    public Dictionary<string, string>? GetValueFromCodeSystemByName(string codeSystemName, string inputValue)
    {
        _logger.LogInformation($"Attempting to get value {inputValue}");

        var fetchedValue = CodeSystems
            .Where(cs => cs.Key == codeSystemName)
            .Select(cs => cs.Value.GetValueSystemOid(inputValue))
            .ToDictionary(gob => gob!.Value.Key, gob => gob!.Value.Value);

        if (fetchedValue != null)
        {
            _logger.LogInformation($"Got {fetchedValue?.Count ?? 0} values");

            return fetchedValue;
        }

        _logger.LogWarning($"Could not find value {inputValue}");
        return null;
    }

    public KeyValuePair<string, string>? GetValueFromCodeSystem(ComprehensiveCodeSystem[]? codeSystems, string inputValue)
    {
        _logger.LogInformation($"Getting value {inputValue} from code systems {string.Join(", ", codeSystems?.Select(cc => cc.SystemOid) ?? [])}");

        var fetchedValue = codeSystems.GetValueSystemOid(inputValue);

        if (fetchedValue != null)
        {
            _logger.LogInformation($"Got value {fetchedValue?.Value} from code system {fetchedValue?.Key}");

            return fetchedValue;
        }
        _logger.LogWarning($"Could not find value {inputValue} in code systems {string.Join(", ", codeSystems?.Select(cc => cc.SystemOid) ?? [])}");
        return null;
    }
}
