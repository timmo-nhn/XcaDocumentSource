using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class INT_POS : INT
{
    public INT_POS() { }

    public new int? Value
    {
        get { return base.Value; }
        set
        {
            if (value < 1)
                throw new ArgumentException("The value must be a positive integer.");
            base.Value = value;
        }
    }
}
