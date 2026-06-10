using System.Text.Json;
using System.Text.Json.Serialization;

namespace XcaXds.Terminology;

public static class TerminologyConstants
{
    public static readonly JsonSerializerOptions JsonSerializerDefaultSettings = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };

    /// <summary>
    /// The internal names of the code systems referenced by the XDS metadata specification will use. 
    /// These are not the same as the OIDs or URLs used in the XDS specification, but rather internal identifiers for the code systems that will be used in the application.
    /// </summary>
    public static class XdsCodeSystemNames
    {
        public const string ConfidentialityCode = "ConfidentialityCode";
        public const string Gender = "Gender";
        public const string ClassCode = "ClassCode";
        public const string TypeCode = "TypeCode";
        public const string EventCode = "EventCode";
        public const string FacilityType = "FacilityType";
        public const string PracticeSettingCode = "PracticeSettingCode";

    }

    /// <summary>
    /// The internal names of the code systems referenced by the XDS metadata specification will use. 
    /// </summary>
    public static class OtherCodeSystemNames
    {
        public const string OrganizationAssigningAuthorities = "OrganizationAssigningAuthorities";
        public const string PatientAssigningAuthorities = "PatientAssigningAuthorities";
        public const string PractitionerAssigningAuthorities = "PractitionerAssigningAuthorities";
    }
}