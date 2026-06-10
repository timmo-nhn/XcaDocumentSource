namespace XcaXds.Terminology;

public static class CodeSystemNames
{
    /// <summary>
    /// The internal names of the code systems referenced by the XDS metadata specification will use. 
    /// These are not the same as the OIDs or URLs used in the XDS specification, but rather internal identifiers for the code systems that will be used in the application.
    /// </summary>
    public static class XdsCodeSystems
    {
        public const string ConfidentialityCode = "ConfidentialityCode";
        public const string Gender = "Gender";
        public const string ClassCode = "ClassCode";
        public const string TypeCode = "TypeCode";
        public const string EventCode = "EventCode";
        public const string FacilityType = "FacilityType";
        public const string PracticeSettingCode = "PracticeSettingCode";
        public const string FormatCode = "FormatCode";
    }

    public static class Hl7CodeSystems
    {
        public const string Attachments = "Attachments";
    }

    public static class AuthenticationCodeSystems
    {
        public const string PurposeOfUse = "PurposeOfUse";
        public const string Acp = "Acp";
    }

    /// <summary>
    /// The internal names of the code systems referenced by the XDS metadata specification will use. 
    /// </summary>
    public static class OtherCodeSystems
    {
        public const string OrganizationAssigningAuthorities = "OrganizationAssigningAuthorities";
        public const string PatientAssigningAuthorities = "PatientAssigningAuthorities";
        public const string PractitionerAssigningAuthorities = "PractitionerAssigningAuthorities";
    }
}