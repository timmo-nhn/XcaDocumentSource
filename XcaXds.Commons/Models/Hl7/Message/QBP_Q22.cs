
using NHapi.Base.Log;
using NHapi.Base;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Model.V25.Segment;
using NHapi.Model.V25.Message;

namespace XcaXds.Commons.Models.Hl7.Message;

[Serializable]
public class QBP_Q22 : AbstractMessage
{
    public QPD? QPD { get; set; }
    public RCP? RCP { get; set; }

    public QBP_Q22(IModelClassFactory factory) : base(factory)
    {
        this.add(typeof(MSH), true, false);
        this.add(typeof(QPD), true, false);
        this.add(typeof(RCP), true, false);
    }
}
