using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.Models.Finnkode;

namespace XcaXds.Terminology.Mappers;

public class FinnKodeMapper : IValueSetMapper<ValueSetCodeList>
{
    public const string OidPrefix = "2.16.578.1.12.4.1.1";

    public ComprehensiveCodeSystem MapToComprehensiveCodeSystem(ValueSetCodeList codeSystem)
    {

        var values = new List<ValueSetValue>();

        foreach (var codeValue in codeSystem.CodeValues ?? [])
        {
            if(codeValue.Value != null)
                values.Add(new(codeValue.Value, codeValue.Name));
        }

        return new ComprehensiveCodeSystem(OidPrefix + "." + codeSystem.Id, values.ToArray());
    }
}