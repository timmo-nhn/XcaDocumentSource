namespace XcaXds.Commons.Codes;

public static class CodeSystemNamingDevice
{
    private static readonly Dictionary<string, string> _listOfNames = new()
    {
        { "urn:oid:2.16.578.1.12.4.1.1.8451", "Fagområde" },
        { "urn:oid:2.16.578.1.12.4.1.1.8627", "Tjenestetyper innen spesialisthelsetjenesten" },
        { "urn:oid:2.16.578.1.12.4.1.1.8655", "Helsehjelpsområde" },
        { "urn:oid:2.16.578.1.12.4.1.1.8662", "Fylkeskommunale tjenestetyper" },
        { "urn:oid:2.16.578.1.12.4.1.1.8663", "Tjenestetyper for kommunal helse- og omsorgstjeneste mv." },
        { "urn:oid:2.16.578.1.12.4.1.1.8664", "Tjenestetyper for apotek og bandasjister" },
        { "urn:oid:2.16.578.1.12.4.1.1.8666", "Felles tjenestetyper" },
        { "urn:oid:2.16.578.1.12.4.1.1.8668", "Tjenestetyper for spesialisthelsetjenesten" },
        { "urn:oid:2.16.578.1.12.4.1.1.9151", "Tjenestetype i helse- og omsorgstjenesten" },
        { "urn:oid:2.16.578.1.12.4.1.1.9060", "Kategori helsepersonell" },
        { "urn:oid:2.16.840.1.113883.1.11.20448", "PurposeOfUse (HL7)" },
        { "urn:AuditEventHL7Norway/CodeSystem/carerelation", "Care relation (HL7 Norway)" },
        { "1.0.14265.1", "ISO 14265 Classification of Purposes for processing personal health information" },
    };

    public static string GetNameFromCodeOrDefault(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return default!;
        }

        if (_listOfNames.TryGetValue(code, out var result))
        {
            return result;
        }

        throw new Exception($"Invalid code system '{code}' encountered.");
    }
}
