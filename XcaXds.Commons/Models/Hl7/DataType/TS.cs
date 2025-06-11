namespace XcaXds.Commons.Models.Hl7.DataType;

public class TS : Hl7Object
{
    //Format: YYYY[MM[DD[HH[MM[SS[.S[S[S[S]]]]]]]]][+/ -ZZZZ]^<degree of precision>
    public string TimeOfEvent { get; set; }
    public string PrecisionDegree { get; set; }
}