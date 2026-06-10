using System.Text.Json;
using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Sources;

public class FileBasedJsonMapper : ICodeSystemMapper
{
    public FileBasedJsonMapper()
    {
    }

    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string rawInput)
    {
        return JsonSerializer.Deserialize<ComprehensiveCodeSystem>(rawInput, TerminologyConstants.JsonSerializerDefaultSettings);
    }
}