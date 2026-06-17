using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;

namespace XcaXds.Terminology.ValueSetMappers.XcaDocumentSource;

public class StringBasedMapper : ICodeSystemMapper
{
    private string? _separator;
    private string _system;

    public StringBasedMapper(string? separator, string system)
    {
        _separator = separator;
        _system = system;
    }

    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string rawInput)
    {
        var values = _separator == null ? new[] { rawInput } : rawInput.Split(_separator);

        return new(_system, [new(rawInput)]);
    }
}