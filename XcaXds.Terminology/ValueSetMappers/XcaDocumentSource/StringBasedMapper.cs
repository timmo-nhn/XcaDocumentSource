using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;

namespace XcaXds.Terminology.ValueSetMappers.XcaDocumentSource;

public class StringBasedMapper : ICodeSystemMapper
{
    private string _system;

    private string[]? _values;

    public StringBasedMapper(string[]? values, string system)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        _values = values;
        _system = system;
    }

    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string _)
    {
        // The codesystem is predefined in the constructor, so we just
        // ignore the input parameter and use the predefined values and system.
        var values = _values?.Select(v => new CodeSystemValue(v)) ?? throw new InvalidOperationException("Values cannot be null");

        return new(_system, [.. values]);
    }
}