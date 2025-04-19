using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Model.V25.Segment;

namespace XcaXds.Commons.Models.Hl7.Message;

public class QBP_Q22 // : AbstractMessage
{
    //public QBP_Q22(IModelClassFactory theFactory) : base(theFactory)
    //{
    //}

    public MSH MSH { get; set; }
    public SFT SFT { get; set; }
    public QPD QPD { get; set; }
    public RCP RCP { get; set; }
    public DSC DSC { get; set; }
}
