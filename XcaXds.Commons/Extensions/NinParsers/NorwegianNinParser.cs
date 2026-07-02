using System.Globalization;
using XcaXds.Commons.Extensions.NinParsers;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;

namespace XcaXds.Commons.Extensions.No;

public class NorwegianNinParser : INinParser
{
    private readonly TerminologyService _terminologyService;

    private ComprehensiveCodeSystem[] _ninSystems;

    public NorwegianNinParser(TerminologyService terminologyService)
    {
        _terminologyService = terminologyService;
        _ninSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.PersonAssigningAuthorities);
        _nin = _ninSystems.GetFirstValueByName("NIN")!;
        _tNin = _ninSystems.GetFirstValueByName("TNIN")!;
        _eNin = _ninSystems.GetFirstValueByName("ENIN")!;
    }

    private string _nin { get; init; }
    private string _tNin { get; init; }
    private string _eNin { get; init; }

    public bool CanHandle(string inputNin)
    {
        return ParseNinToCxWithAssigningAuthority(inputNin) != null;
    }

    /// <summary>
    /// Parse a National Identifier Number and get the birth date aswell as the proper assigning authority depending on whether its a Dnr, Hnr or Pnr/Fnr)<para/>
    /// </summary>
    public CX? ParseNinToCxWithAssigningAuthority(string? inputNin)
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
                oid.UniversalId = _tNin;
            }
            else
            {
                oid.UniversalId = _nin;
            }
        }

        // Normal D-number = +40 on day
        else if (int.Parse(day) - 40 is > 0 and <= 31)
        {
            oid.UniversalId = _tNin;
        }

        // Normal H-number = +40 on month
        else if (int.Parse(month) - 40 is > 0 and <= 12)
        {
            oid.UniversalId = _eNin;
        }
        else
        {
            oid.UniversalId = _nin;
        }

        return new CX()
        {
            IdNumber = inputNin,
            AssigningAuthority = oid
        };
    }

    public DateTime? ParseNinToDateTime(string? patientIdentifier)
    {
        return ParseNinToDateTime(ParseNinToCxWithAssigningAuthority(patientIdentifier));
    }

    public DateTime? ParseNinToDateTime(CX? patientCx)
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
            case var fnr when fnr == _nin:
                break;

            case var dnr when dnr == _tNin:
                day = (int.Parse(day) - 40).ToString();
                break;

            case var hnr when hnr == _eNin:
                month = (int.Parse(month) - 40).ToString();
                break;

            default:
                break;
        }

        return DateTime.Parse($"{month}/{day}/{century}{year}", CultureInfo.InvariantCulture);
    }

    public int GetAgeFromPatientId(string? patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId) || patientId.Length != 11) return 0;

        var patientNin = ParseNinToDateTime(patientId);

        var year = DateTime.Today.Year - (patientNin.HasValue ? patientNin.Value.Year : 0);

        return year;
    }
}
