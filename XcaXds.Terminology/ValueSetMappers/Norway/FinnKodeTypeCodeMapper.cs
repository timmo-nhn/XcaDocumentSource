using System.Text.Json;
using XcaXds.Shared;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;
using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.Models.Finnkode;

namespace XcaXds.Terminology.ValueSetMappers.Norway;

public sealed class FinnKodeTypeCodeMapper : ICodeSystemMapper
{
    public const string OidPrefix = "2.16.578.1.12.4.1.1";

    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string rawInput)
    {
        var codeSystem = JsonSerializer.Deserialize<ValueSetCodeList>(rawInput, Constants.JsonDefaultOptions.DefaultSettings);

        if (codeSystem == null) return null;

        var values = new List<CodeSystemValue>();

        foreach (var codeValue in codeSystem.CodeValues ?? [])
        {
            if (codeValue.Value != null && codeValue.Value.EndsWith("00-1") == false)
                values.Add(new(codeValue.Value, codeValue.Name));
        }

        return new ComprehensiveCodeSystem(OidPrefix + "." + codeSystem.Id, values.ToArray());
    }
}