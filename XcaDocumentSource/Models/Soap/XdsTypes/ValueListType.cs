using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class ValueListType
{
    [XmlElement("Value", Order = 0)]
    public string[] Value;

}
