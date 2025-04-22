using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Model.V25.Datatype;
using NHapi.Model.V25.Segment;
using NHapi.Base.Log;

public class RSP_K22 : AbstractMessage
{
    public RSP_K22() : base(new DefaultModelClassFactory())
    {
        this.Add(typeof(MSH), true, false);
        this.Add(typeof(SFT), false, true);
        this.Add(typeof(MSA), true, false);
        this.Add(typeof(QAK), true, false);
        this.Add(typeof(QPD), true, false);
        this.Add(typeof(QUERY_RESPONSE), false, true);
        this.Add(typeof(DSC), false, false);
    }

    public MSH MSH => (MSH)this.GetStructure("MSH");
    public MSA MSA => (MSA)this.GetStructure("MSA");
    public QAK QAK => (QAK)this.GetStructure("QAK");
    public QPD QPD => (QPD)this.GetStructure("QPD");
    public DSC DSC => (DSC)this.GetStructure("DSC");

    public class QUERY_RESPONSE : AbstractGroup
    {
        public QUERY_RESPONSE(IGroup parent, IModelClassFactory factory) : base(parent, factory)
        {
            this.Add(typeof(PID), true, false);
            this.Add(typeof(PD1), false, false);
            this.Add(typeof(NK1), false, true);
            this.Add(typeof(QRI), false, false);
        }

        public PID PID => (PID)this.GetStructure("PID");
        public PD1 PD1 => (PD1)this.GetStructure("PD1");
        public NK1 GetNK1(int rep) => (NK1)this.GetStructure("NK1", rep);
        public int NK1Reps => this.GetAll("NK1").Length;
        public QRI QRI => (QRI)this.GetStructure("QRI");
    }

    public QUERY_RESPONSE GetQUERY_RESPONSE(int rep) => (QUERY_RESPONSE)this.GetStructure("QUERY_RESPONSE", rep);
    public int QUERY_RESPONSEReps => this.GetAll("QUERY_RESPONSE").Length;
}
