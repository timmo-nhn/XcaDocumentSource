﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument;


[Serializable]
[XmlType("author", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Author
{
}
