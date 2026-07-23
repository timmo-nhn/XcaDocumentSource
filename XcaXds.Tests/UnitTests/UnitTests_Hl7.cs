using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;

namespace XcaXds.Tests.UnitTests;

public class UnitTests_Hl7
{
    [Fact]
    public void SerializePid_WithNestedCxAndHd_UsesCollisionSafeNestedSeparators()
    {
        var pid = new PID(new CX("12345678910", "2.16.578.1.12.4.1.4.1"));

        var serialized = pid.Serialize(Constants.Hl7.Separator.Ampersand);

        Assert.Equal("&&12345678910^^^|2.16.578.1.12.4.1.4.1|ISO", serialized);
    }

    [Fact]
    public void ParsePid_WithNestedCxAndHd_RoundTripsCollisionSafeSeparators()
    {
        const string serializedPid = "&&12345678910^^^|2.16.578.1.12.4.1.4.1|ISO";

        var parsed = Hl7Object.Parse<PID>(serializedPid, Constants.Hl7.Separator.Ampersand);

        Assert.NotNull(parsed?.PatientIdentifier);
        Assert.Equal("12345678910", parsed?.PatientIdentifier?.IdNumber);
        Assert.Equal("2.16.578.1.12.4.1.4.1", parsed?.PatientIdentifier?.AssigningAuthority?.UniversalId);
        Assert.Equal(Constants.Hl7.UniversalIdType.Iso, parsed?.PatientIdentifier?.AssigningAuthority?.UniversalIdType);
    }
}