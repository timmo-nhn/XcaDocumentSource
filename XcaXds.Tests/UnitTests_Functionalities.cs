using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Tests;

public class UnitTests_Functionalities
{
    [Fact]
    public async Task ParseNorwegianNationalIdentifiers()
    {
        var nins = new[]
        {
            new { Value = "05020279712", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("05.02.2002") }, // (05.02.2002) Tim Fnr
            new { Value = "13116900216", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - Normal Fnr
            new { Value = "53116900216", AssignedAuthority = Constants.Oid.Dnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - D-number: Day + 40        
            new { Value = "13516900216", AssignedAuthority = Constants.Oid.Hnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - H-number: Month + 40
            new { Value = "13916900216", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - +80 Synthetic Normal
            new { Value = "53916900216", AssignedAuthority = Constants.Oid.Dnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - +80 Synthetic D-number: Day + 40
            new { Value = "13766900216", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - +65 Synthetic Normal
            new { Value = "53766900216", AssignedAuthority = Constants.Oid.Dnr, Excpected = DateTime.Parse("13.11.1969") }, // (13.11.1969) Line Danser - +65 Synthetic D-number: Day + 40
            new { Value = "02835998374", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("02.03.1959") }, // (02.03.1959) AUTORISERT JUL
            new { Value = "17855599120", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("17.05.1955") }, // (17.05.1955) USNOBBET KLOKKE
            new { Value = "08777634659", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("08.12.1976") }, // (08.12.1976) ULYDIG BOLLE
            new { Value = "29870049887", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("29.07.1900") }, // (29.07.1900) SIVILISERT ANTILOPE
            new { Value = "16910948990", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("16.11.1909") }, // (16.11.1909) BARMHJERTIG BØK
            new { Value = "22810999865", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("22.01.2009") }, // (22.01.2009) FORSKJELLIG ANALYSE
            new { Value = "09838973652", AssignedAuthority = Constants.Oid.Fnr, Excpected = DateTime.Parse("09.03.1889") }, // (09.03.1889) RUND JUKEBOKS
        };

        foreach (var nin in nins)
        {
            var parsedNin = Hl7FhirExtensions.ParseNorwegianNinToCxWithAssigningAuthority(nin.Value);

            var dateTimeFromNin = Hl7FhirExtensions.ParseNorwegianNinToDateTime(parsedNin);

            Assert.Equal(nin.Excpected, dateTimeFromNin);

            Assert.Equal(nin.AssignedAuthority, parsedNin?.AssigningAuthority?.UniversalId);
        }
    }

    [Fact]
    public async Task UniqueGuid()
    {
        var guid = Guid.NewGuid().ToString();
        var guidFirstPart = guid.Substring(0, 6);

        var secondGuid = Guid.NewGuid().ToString();
        var secondGuidFirstPart = secondGuid.Substring(0, 6);

        var counter = 0;

        while (!string.Equals(guidFirstPart, secondGuidFirstPart))
        {
            secondGuid = Guid.NewGuid().ToString();
            secondGuidFirstPart = secondGuid.Substring(0, 6);
            counter++;
        }
    }
}
