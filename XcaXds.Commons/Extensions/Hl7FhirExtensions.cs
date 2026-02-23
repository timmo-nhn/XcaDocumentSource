using Hl7.Fhir.Model;
using System.Globalization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Commons.Extensions;

public static class Hl7FhirExtensions
{
    public static DateRange GetDateTimeRangeFromDateParameters(string timingAndDate)
    {
        timingAndDate = timingAndDate.Replace("%3A", ":");
        var modifier = timingAndDate.Substring(0, 2);
        var date = timingAndDate.Substring(2);
        var datetime = DateTime.Parse(date);

        switch (modifier)
        {
            case "eq":
                var queryInstantEq = DateTime.Parse(date);
                return new DateRange(queryInstantEq, queryInstantEq.AddDays(1).Trim(TimeSpan.TicksPerDay).AddTicks(-1));

            case "gt":
                var queryInstantGt = DateTime.Parse(date);
                return new DateRange(queryInstantGt, null);

            case "lt":
                var queryInstantLt = DateTime.Parse(date);
                return new DateRange(null, queryInstantLt);

            case "ge":
                var queryInstantGe = DateTime.Parse(date);
                return new DateRange(queryInstantGe.AddTicks(-1), null);

            case "le":
                var queryInstantLe = DateTime.Parse(date);
                return new DateRange(null, queryInstantLe.AddTicks(-1));

            case "sa":
                var queryInstantSa = DateTime.Parse(date);
                return new DateRange(queryInstantSa, null);

            case "eb":
                var queryInstantEb = DateTime.Parse(date);
                return new DateRange(null, queryInstantEb.AddTicks(-1));

            case "ap":
                var queryInstantAp = DateTime.Parse(date);
                return new DateRange(queryInstantAp.AddDays(-10), queryInstantAp.AddDays(10));

            default:
                break;
        }


        throw new NotImplementedException();
    }

    static DateTime Trim(this DateTime date, long roundTicks)
    {
        return new DateTime(date.Ticks - date.Ticks % roundTicks, date.Kind);
    }

    /// <summary>
    /// Parse a National Identifier Number and get the birth date aswell as the proper assigning authority depending on whether its a Dnr, Hnr or Pnr/Fnr)
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
            if (int.Parse(day) - 50 is > 0 and <= 31)
            {
                oid.UniversalId = Constants.Oid.Dnr;
            }
            else
            {
                oid.UniversalId = Constants.Oid.Fnr;
            }
        }

        // Normal D-number = +40 on day
        else if (int.Parse(day) - 40 is > 0 and <= 31)
        {
            oid.UniversalId = Constants.Oid.Dnr;
        }

        // Normal H-number = +40 on month
        else if (int.Parse(month) - 40 is > 0 and <= 12)
        {
            oid.UniversalId = Constants.Oid.Hnr;
        }
        else
        {
            oid.UniversalId = Constants.Oid.Fnr;
        }

        return new CX()
        {
            IdNumber = inputNin,
            AssigningAuthority = oid
        };
    }

    public static ResourceReference GetResourceAsResourceReference(Resource resource)
    {
        return new ResourceReference() { Reference = $"#{resource.Id}" };
    }

    public static List<ResourceReference> GetResourceAsResourceReference(List<Resource> resource)
    {
        return resource.Select(res => GetResourceAsResourceReference(res)).ToList();
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

            _ => throw new Exception($"Invalid FNR: {patientCx.Serialize()}")
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
            case Constants.Oid.Fnr:
                break;

            case Constants.Oid.Dnr:
                day = (int.Parse(day) - 40).ToString();
                break;

            case Constants.Oid.Hnr:
                month = (int.Parse(month) - 40).ToString();
                break;

            default:
                break;
        }

        return DateTime.Parse($"{month}/{day}/{century}{year}", CultureInfo.InvariantCulture);
    }
}