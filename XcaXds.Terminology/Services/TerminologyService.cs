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
            _logger.LogInformation("Adding to existing code system '{name}' with {count} entries...", name, codeSystems.Length);
            CodeSystems[name] = existingCodeSystems.Concat(codeSystems).ToArray();
        }
        else
        {
            _logger.LogInformation("Adding code system '{name}' with {count} entries...", name, codeSystems.Length);
            CodeSystems.Add(name, codeSystems);
        }

        _logger.LogInformation("Added code system '{name}' with {count} entries", name, codeSystems.Length);
    }

    public ComprehensiveCodeSystem[] GetCodeSystemByKey(string name)
    {
        _logger.LogDebug("Getting code system by name '{name}'", name);
        return CodeSystems[name];
    }

    public ComprehensiveCodeSystem[] GetCodeSystemBySystem(string system)
    {
        return CodeSystems.Values.SelectMany(cs => cs).Where(cs => cs.System == system || cs.SystemsAlternate?.Any(aSys => aSys == system) == true).ToArray();
    }

    public string[]? GetValueFromCodeSystemByName(string codeSystemName, string inputValue)
    {
        _logger.LogDebug("Attempting to get Value from Name '{inputValue}' from System '{codeSystemName}'", inputValue, codeSystemName);

        var fetchedSystems = CodeSystems.TryGetValue(codeSystemName, out var codeSys) ? codeSys : null;

        if (fetchedSystems != null)
        {
            _logger.LogDebug("Fetched '{count}' CodeSystems", fetchedSystems?.Length);

            var eligibleValue = fetchedSystems?
                .SelectMany(cs => cs.Values ?? [])
                .Where(cs => cs.Name == inputValue)
                .ToArray();

            if (eligibleValue is { Length: > 0 } s)
            {
                var values = s.Select(ev => ev.Value).OfType<string>().ToArray();
                _logger.LogDebug("Got {count} value{plural} ({values})", s.Length, s.Length > 1 ? "s" : "", string.Join(' ', values));

                return values;
            }
        }

        _logger.LogDebug("Could not find value '{inputValue}' from '{codeSystemName}'", inputValue, codeSystemName);
        return null;
    }

    public string[]? GetValueFromCodeSystemByName(string codeSystemName, params string[] inputValues)
    {
        _logger.LogDebug("Attempting to get Value from Name '{inputValues}' from System '{codeSystemName}'", string.Join(", ", inputValues), codeSystemName);

        var fetchedSystems = CodeSystems.TryGetValue(codeSystemName, out var codeSys) ? codeSys : null;

        if (fetchedSystems != null)
        {
            _logger.LogDebug("Fetched '{count}' CodeSystems", fetchedSystems?.Length);

            var eligibleValue = fetchedSystems?
                .SelectMany(cs => cs.Values ?? [])
                .Where(cs => inputValues.Contains(cs.Name))
                .ToArray();

            if (eligibleValue is { Length: > 0 } s)
            {
                var values = s.Select(ev => ev.Value).OfType<string>().ToArray();
                _logger.LogDebug("Got {count} value{plural} ({values})", s.Length, s.Length > 1 ? "s" : "", string.Join(' ', values));

                return values;
            }
        }

        _logger.LogDebug("Could not find value '{inputValues}' from '{codeSystemName}'", string.Join(", ", inputValues), codeSystemName);
        return null;
    }

    public KeyValuePair<string, string>? GetValueFromCodeSystem(ComprehensiveCodeSystem[]? codeSystems, string inputValue)
    {
        _logger.LogDebug("Getting value '{inputValue}' from code systems '{codeSystems}'", inputValue, string.Join(", ", codeSystems?.Select(cc => cc.System) ?? []));

        var fetchedValue = codeSystems.GetByValueSystem(inputValue);

        if (fetchedValue != null)
        {
            _logger.LogDebug("Got value '{value}' from code system '{key}'", fetchedValue?.Value, fetchedValue?.Key);

            return fetchedValue;
        }

        _logger.LogWarning("Could not find value '{inputValue}' in code systems '{codeSystems}'", inputValue, string.Join(", ", codeSystems?.Select(cc => cc.System) ?? []));

        return null;
    }
}