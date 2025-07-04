using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Tests;

public class UnitTests_Functionalities
{
    [Fact]
    public async Task ParseNinToCxWithAssigningAuthority()
    {
        var nin = string.Empty;
        var parsedNin = new CX();

        // Normal Fnr
        nin = "13116900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Fnr);
        

        // D-number: Day + 40
        nin = "53116900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Dnr);


        // H-number: Month + 40
        nin = "13516900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Hnr);
        

        // +80 Synthetic Normal
        nin = "13816900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Fnr);


        // +80 Synthetic D-number: Day + 40
        nin = "53816900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Dnr);
        

        // +65 Synthetic Normal
        nin = "13766900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Fnr);


        // +65 Synthetic D-number: Day + 40
        nin = "53766900216";
        parsedNin = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(nin);
        Assert.Equal(parsedNin.AssigningAuthority.UniversalId, Constants.Oid.Dnr);
    }
}
