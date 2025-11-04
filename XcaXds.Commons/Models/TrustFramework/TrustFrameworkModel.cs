using System.Text.Json.Serialization;

namespace XcaXds.Commons.Models.TrustFramework;

public class SystemWithNameAndAuthority
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}

public class SystemWithAuthority
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}

public class SystemWithCodeAndAssigner
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;
    [JsonPropertyName("assigner")]
    public string Assigner { get; set; } = string.Empty;
}

public class DecisionRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("user_selected")]
    public bool UserSelected { get; set; }
}

public class Practitioner
{
    [JsonPropertyName("identifier")]
    public SystemWithNameAndAuthority Identifier { get; set; } = default!;
    [JsonPropertyName("hpr_nr")]
    public SystemWithAuthority? HprNr { get; set; }
    [JsonPropertyName("authorization")]
    public SystemWithCodeAndAssigner? Authorization { get; set; }
    [JsonPropertyName("legal_entity")]
    public SystemWithNameAndAuthority LegalEntity { get; set; } = default!;
    [JsonPropertyName("point_of_care")]
    public SystemWithNameAndAuthority PointOfCare { get; set; } = default!;
    [JsonPropertyName("department")]
    public SystemWithNameAndAuthority? Department { get; set; }
};

public class CareRelationship
{
    [JsonPropertyName("healthcare_service")]
    public SystemWithCodeAndAssigner? HealthcareService { get; set; }
    [JsonPropertyName("purpose_of_use")]
    public SystemWithCodeAndAssigner PurposeOfUse { get; set; } = default!;
    [JsonPropertyName("purpose_of_use_details")]
    public SystemWithCodeAndAssigner? PurposeOfUseDetails { get; set; }
    [JsonPropertyName("decision_ref")]
    public DecisionRef DecisionRef { get; set; } = default!;
};

public class Patient
{
    [JsonPropertyName("point_of_care")]
    public SystemWithNameAndAuthority? PointOfCare { get; set; }
    [JsonPropertyName("department")]
    public SystemWithNameAndAuthority? Department { get; set; }
}

public class PatientArray : List<Patient>
{
    public List<Patient> Patients { get; set; } = default!;
}

public class TrustFrameworkModel
{
    [JsonPropertyName("practitioner")]
    public Practitioner Practitioner { get; set; } = default!;

    [JsonPropertyName("care_relationship")]
    public CareRelationship CareRelationship { get; set; } = default!;

    [JsonPropertyName("patients")]
    public PatientArray Patients { get; set; } = default!;
}
