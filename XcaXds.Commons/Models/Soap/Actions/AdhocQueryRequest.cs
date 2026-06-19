using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0")]
[XmlRoot(Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0", IsNullable = false)]
[DebuggerDisplay("AdhocQueryRequest: {GetDisplay()}")]
public partial class AdhocQueryRequest : RegistryRequestType
{
    private string GetDisplay() =>
        $"{Id} ({ConstantsExtensions.GetAsDictionary(typeof(Constants.Xds.StoredQueries)).FirstOrDefault(kv => kv.Value == Id)})";

    public AdhocQueryRequest()
    {
        Federated = false;
        StartIndex = "0";
        MaxResults = "-1";
    }

    [XmlElement]
    public ResponseOptionType? ResponseOption { get; set; }

    [XmlElement(Namespace = Constants.Xds.Namespaces.Rim)]
    public AdhocQueryType AdhocQuery { get; set; } = new();

    [XmlAttribute(AttributeName = "federated")]
    [DefaultValue(false)]
    public bool Federated;

    [XmlAttribute(AttributeName = "federation", DataType = "anyURI")]
    public string? Federation;

    [XmlAttribute(AttributeName = "startIndex", DataType = "integer")]
    [DefaultValue("0")]
    public string StartIndex;

    [XmlAttribute(AttributeName = "maxResults", DataType = "integer")]
    [DefaultValue("-1")]
    public string MaxResults;
}
