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
        if (CodeSystems.ContainsKey(name))
        {
            _logger.LogInformation($"Adding to existing code system {name} with {codeSystems.Length} entries...");
            CodeSystems[name] = CodeSystems[name].Concat(codeSystems).ToArray();
        }
        else
        {
            _logger.LogInformation($"Adding code system {name} with {codeSystems.Length} entries...");
            CodeSystems.Add(name, codeSystems);
        }

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

    public string[]? GetValueFromCodeSystemByName(string codeSystemName, string inputValue)
    {
        _logger.LogInformation($"Attempting to get Value from Name {inputValue} in System {codeSystemName}");

        var fetchedSystems = CodeSystems.TryGetValue(codeSystemName, out var codeSys) ? codeSys : null;

        if (fetchedSystems != null)
        {
            _logger.LogInformation($"Got {fetchedSystems?.Length} CodeSystems");

            var eligibleValue = fetchedSystems?
                .SelectMany(cs => cs.Values ?? [])
                .Where(cs => cs.Name == inputValue)
                .ToArray();

            if (eligibleValue is { Length: > 0 } s)
            {
                var values = s.Select(ev => ev.Value).OfType<string>().ToArray();
                _logger.LogInformation($"Got {s.Length} value{(s.Length > 1 ? "s" : "")} ({string.Join(' ', values)})");

                return values;
            }
        }

        _logger.LogInformation($"Could not find value {inputValue} from {codeSystemName}");
        return null;
    }

    public KeyValuePair<string, string>? GetValueFromCodeSystem(ComprehensiveCodeSystem[]? codeSystems, string inputValue)
    {
        _logger.LogInformation($"Getting value {inputValue} from code systems {string.Join(", ", codeSystems?.Select(cc => cc.SystemOid) ?? [])}");

        var fetchedValue = codeSystems.GetByValueOid(inputValue);

        if (fetchedValue != null)
        {
            _logger.LogInformation($"Got value {fetchedValue?.Value} from code system {fetchedValue?.Key}");

            return fetchedValue;
        }
        _logger.LogWarning($"Could not find value {inputValue} in code systems {string.Join(", ", codeSystems?.Select(cc => cc.SystemOid) ?? [])}");
        return null;
    }
}