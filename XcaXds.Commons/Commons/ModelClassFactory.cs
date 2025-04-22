using NHapi.Base.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcaXds.Commons.Models.Hl7.Message;

namespace XcaXds.Commons.Commons;

public class CustomModelClassFactory : DefaultModelClassFactory
{
    public override Type GetMessageClass(string theName, string theVersion, bool isExplicit)
    {
        if (theVersion == "2.5" && theName == "QBP_Q21")
        {
            // Field MSH-9-Message Type shall have all three components populated with a value.
            // The first component shall have a value of QBP ;
            // the second component shall have a value of Q22 . The third component it shall have a value of QBP_Q21.
            // ( ...hrnngh )

            return typeof(QBP_Q22); 
        }

        return base.GetMessageClass(theName, theVersion, isExplicit);
    }
}
