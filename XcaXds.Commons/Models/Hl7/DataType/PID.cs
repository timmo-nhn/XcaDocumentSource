namespace XcaXds.Commons.Models.Hl7.DataType;

public class PID : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string SetId { get; set; } = string.Empty;

    [Hl7(Sequence = 2)]
    public CX? PatientId { get; set; }

    [Hl7(Sequence = 3)]
    public CX? PatientIdentifier { get; set; }

    [Hl7(Sequence = 4)]
    public CX? AlternatePatientId { get; set; }

    [Hl7(Sequence = 5)]
    public XPN? PatientName { get; set; }

    [Hl7(Sequence = 6)]
    public XPN? MothersMaidenName { get; set; }

    [Hl7(Sequence = 7)]
    public DateTime BirthDate { get; set; }

    [Hl7(Sequence = 8)]
    public string Gender { get; set; } = string.Empty;
}
