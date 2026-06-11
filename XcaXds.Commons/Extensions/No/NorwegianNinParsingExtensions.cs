using System.Globalization;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Extensions.No;

/// HAYO! Change semantics of the functions or factor this out to a more reusable thing that not so Norway-specific?!
public static class NorwegianNinParsingExtensions
{
    public const string Fnr = "2.16.578.1.12.4.1.4.1";
    public const string Dnr = "2.16.578.1.12.4.1.4.2";
    public const string Hnr = "2.16.578.1.12.4.1.4.3";

    /// <summary>
    /// Parse a National Identifier Number and get the birth date aswell as the proper assigning authority depending on whether its a Dnr, Hnr or Pnr/Fnr)<para/>
    /// </summary>
    public static CX? ParseNorwegianNinToCxWithAssigningAuthority(string? inputNin)
    {
        if (inputNin?.Length != 11) return null;

        var day = inputNin.Substring(0, 2);
        var month = inputNin.Substring(2, 2);
        var year = inputNin.Substring(4, 2);
        var control = inputNin.Substring(6, 3);

        var oid = new HD()
        {
            UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
            UniversalId = string.Empty
        };

        // Check if its a synthetic test-data Nin
        if (int.Parse(month) - 65 is > 0 and <= 12 || int.Parse(month) - 80 is > 0 and <= 12)
        {
            if (int.Parse(day) - 40 is > 0 and <= 31)
            {
                oid.UniversalId = Dnr;
            }
            else
            {
                oid.UniversalId = Fnr;
            }
        }

        // Normal D-number = +40 on day
        else if (int.Parse(day) - 40 is > 0 and <= 31)
        {
            oid.UniversalId = Dnr;
        }

        // Normal H-number = +40 on month
        else if (int.Parse(month) - 40 is > 0 and <= 12)
        {
            oid.UniversalId = Hnr;
        }
        else
        {
            oid.UniversalId = Fnr;
        }

        return new CX()
        {
            IdNumber = inputNin,
            AssigningAuthority = oid
        };
    }

    public static DateTime? ParseNorwegianNinToDateTime(string? patientIdentifier)
    {
        return ParseNorwegianNinToDateTime(ParseNorwegianNinToCxWithAssigningAuthority(patientIdentifier));
    }

    public static DateTime? ParseNorwegianNinToDateTime(CX? patientCx)
    {
        var inputNin = patientCx?.IdNumber;

        if (patientCx == null || inputNin == null) return null;

        var day = inputNin.Substring(0, 2);
        var month = inputNin.Substring(2, 2);
        var year = inputNin.Substring(4, 2);
        var control = inputNin.Substring(6, 3);


        // https://www.matematikk.org/artikkel.html?tid=64296

        var century = (int.Parse(control), int.Parse(year)) switch
        {
            // 1855–1899
            ( >= 500 and <= 749, >= 55) => "18",

            // 1900–1999 (normal case)
            ( >= 0 and <= 499, _) => "19",

            // 1940–1999 (special rule)
            ( >= 900 and <= 999, >= 40) => "19",

            // 2000–2039 (D-number, H-number, synthetic)
            ( >= 500 and <= 999, <= 39) => "20",

            _ => "19"
        };


        // Check if its a synthetic test-data Nin
        if (int.Parse(month) - 65 is > 0 and <= 12)
        {
            month = (int.Parse(month) - 65).ToString();
        }

        if (int.Parse(month) - 80 is > 0 and <= 12)
        {
            month = (int.Parse(month) - 80).ToString();
        }

        switch (patientCx.AssigningAuthority?.UniversalId)
        {
            case Fnr:
                break;

            case Dnr:
                day = (int.Parse(day) - 40).ToString();
                break;

            case Hnr:
                month = (int.Parse(month) - 40).ToString();
                break;

            default:
                break;
        }

        return DateTime.Parse($"{month}/{day}/{century}{year}", CultureInfo.InvariantCulture);
    }
}
