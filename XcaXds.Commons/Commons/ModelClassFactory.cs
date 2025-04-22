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
        if (theVersion == "2.5" && theName == "QBP_Q22")
        {
            return typeof(QBP_Q22); // Your custom class
        }

        return base.GetMessageClass(theName, theVersion, isExplicit);
    }
}
