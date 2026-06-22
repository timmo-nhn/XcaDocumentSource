using Hl7.Fhir.Specification.Terminology;
using Microsoft.Extensions.Logging;
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

    public void AddOrUpdateCodeSystem(string name, ComprehensiveCodeSystem[] codeSystems)
    {
        if (CodeSystems.TryGetValue(name, out var existingCodeSystems))
        {
            _logger.LogInformation($"Adding to existing code system '{name}' with {codeSystems.Length} entries...");
            CodeSystems[name] = existingCodeSystems.Concat(codeSystems).ToArray();
        }
        else
        {
            _logger.LogInformation($"Adding code system '{name}' with {codeSystems.Length} entries...");
            CodeSystems.Add(name, codeSystems);
        }

        _logger.LogInformation($"Added code system '{name}' with {codeSystems.Length} entries");
    }

    public ComprehensiveCodeSystem[] GetCodeSystemByKey(string name)
    {
        _logger.LogDebug($"Getting code system by name '{name}'");
        return CodeSystems[name];
    }

    public ComprehensiveCodeSystem[] GetCodeSystemBySystem(string system)
    {
        return CodeSystems.Values.SelectMany(cs => cs).Where(cs => cs.System == system || cs.SystemsAlternate?.Any(aSys => aSys == system) == true).ToArray();
    }

    public string[]? GetValueFromCodeSystemByName(string codeSystemName, string inputValue)
    {
        _logger.LogDebug($"Attempting to get Value from Name '{inputValue}' from System '{codeSystemName}'");

        var fetchedSystems = CodeSystems.TryGetValue(codeSystemName, out var codeSys) ? codeSys : null;

        if (fetchedSystems != null)
        {
            _logger.LogDebug($"Fetched '{fetchedSystems?.Length}' CodeSystems");

            var eligibleValue = fetchedSystems?
                .SelectMany(cs => cs.Values ?? [])
                .Where(cs => cs.Name == inputValue)
                .ToArray();

            if (eligibleValue is { Length: > 0 } s)
            {
                var values = s.Select(ev => ev.Value).OfType<string>().ToArray();
                _logger.LogDebug($"Got {s.Length} value{(s.Length > 1 ? "s" : "")} ({string.Join(' ', values)})");

                return values;
            }
        }

        _logger.LogDebug($"Could not find value '{inputValue}' from '{codeSystemName}'");
        return null;
    }

    public KeyValuePair<string, string>? GetValueFromCodeSystem(ComprehensiveCodeSystem[]? codeSystems, string inputValue)
    {
        _logger.LogDebug($"Getting value '{inputValue}' from code systems '{string.Join(", ", codeSystems?.Select(cc => cc.System) ?? [])}'");

        var fetchedValue = codeSystems.GetByValueSystem(inputValue);

        if (fetchedValue != null)
        {
            _logger.LogDebug($"Got value '{fetchedValue?.Value}' from code system '{fetchedValue?.Key}'");

            return fetchedValue;
        }

        _logger.LogWarning($"Could not find value '{inputValue}' in code systems '{string.Join(", ", codeSystems?.Select(cc => cc.System) ?? [])}'");

        return null;
    }
}