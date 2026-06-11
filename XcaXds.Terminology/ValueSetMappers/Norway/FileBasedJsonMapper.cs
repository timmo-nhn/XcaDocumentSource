using System.Text.Json;
using XcaXds.Shared.Commons;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.ValueSetMappers.Norway;

public class FileBasedJsonMapper : ICodeSystemMapper
{
    public FileBasedJsonMapper()
    {
    }

    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string rawInput)
    {
        return JsonSerializer.Deserialize<ComprehensiveCodeSystem>(rawInput, Constants.JsonDefaultOptions.DefaultSettings);
    }
}