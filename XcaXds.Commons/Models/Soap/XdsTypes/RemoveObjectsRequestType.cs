﻿using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class RemoveObjectsRequestType
{
    [XmlElement("ObjectRefList", Order = 0)]
    public ObjectRefList? ObjectRefList { get; set; }
}

