using Abc.Xacml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using System.Text;
using System.Xml;
using XcaXds.Commons.Extensions;

namespace XcaXds.Commons.Serializers;
public static class XacmlSerializer
{
    public static string? SerializeXacmlToXml(XacmlContextRequest? request, XmlWriterSettings? options = null)
    {
        if (request == null) throw new ArgumentNullException();

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, options))
        {
            var serializer = new Xacml20ProtocolSerializer();
            serializer.WriteContextRequest(writer, request);
        }

        return sb.ToString();
    }

    public static string? SerializeXacmlToXml(XacmlContextResponse? request, XmlWriterSettings? options = null)
    {
        if (request == null) throw new ArgumentNullException();

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, options))
        {
            var serializer = new Xacml20ProtocolSerializer();
            serializer.WriteContextResponse(writer, request);
        }

        return sb.ToString();
    }

    public static string? SerializeXacmlToXml(XacmlPolicy? request, XmlWriterSettings? options = null)
    {
        if (request == null) throw new ArgumentNullException();

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, options))
        {
            var serializer = new Xacml20ProtocolSerializer();
            serializer.WritePolicy(writer, request);
        }

        return sb.ToString();
    }

    public static string? SerializeXacmlToXml(XacmlPolicySet? request, XmlWriterSettings? options = null)
    {
        if (request == null) throw new ArgumentNullException();

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, options))
        {
            var serializer = new Xacml20ProtocolSerializer();
            serializer.WritePolicySet(writer, request);
        }

        return sb.ToString();
    }


    public static XacmlContextRequest? DeserializeXacmlRequest(string? request)
    {
        if (request == null) throw new ArgumentNullException();

        var bytes = request.GetAsUtf8Bytes();
        using (var ms = new MemoryStream(bytes))
        {
            using (var reader = XmlReader.Create(ms))
            {
                var serialize = new Xacml20ProtocolSerializer();

                var xacml = serialize.ReadContextRequest(reader);
                return xacml;
            }
        }
    }

    public static XacmlContextResponse? DeserializeXacmlResponse(string request)
    {
        if (request == null) throw new ArgumentNullException();

        var bytes = request.GetAsUtf8Bytes();
        using (var ms = new MemoryStream(bytes))
        {
            using (var reader = XmlReader.Create(ms))
            {
                var serialize = new Xacml20ProtocolSerializer();

                var xacml = serialize.ReadContextResponse(reader);
                return xacml;
            }
        }
    }

    public static XacmlPolicy? DeserializeXacmlPolicy(string? request)
    {
        if (request == null) throw new ArgumentNullException();

        var bytes = request.GetAsUtf8Bytes();
        using (var ms = new MemoryStream(bytes))
        {
            using (var reader = XmlReader.Create(ms))
            {
                var serialize = new Xacml20ProtocolSerializer();

                var xacml = serialize.ReadPolicy(reader);
                return xacml;
            }
        }
    }

    public static XacmlPolicySet? DeserializeXacmlPolicySet(string? request)
    {
        if (request == null) throw new ArgumentNullException();

        var bytes = request.GetAsUtf8Bytes();
        using (var ms = new MemoryStream(bytes))
        {
            using (var reader = XmlReader.Create(ms))
            {
                var serialize = new Xacml20ProtocolSerializer();

                var xacml = serialize.ReadPolicySet(reader);
                return xacml;
            }
        }
    }

    ///// <summary>
    ///// For XACML 3.0
    ///// </summary>
    //private static void WriteCategoryAttributes(XmlWriter xmlWriter, IEnumerable<XacmlContextAttributes> categoryAttributes, string category)
    //{
    //    if (categoryAttributes.Count() == 0) return;

    //    foreach (var attribute in categoryAttributes)
    //    {
    //        xmlWriter.WriteStartElement("Attributes");
    //        xmlWriter.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
    //        xmlWriter.WriteAttributeString("Category", category);

    //        foreach (var attr in attribute.Attributes)
    //        {
    //            WriteAttribute(xmlWriter, attr);
    //        }

    //        xmlWriter.WriteEndElement();
    //    }
    //}

    //private static void WriteSubject(XmlWriter xmlWriter, ICollection<XacmlContextSubject> subjects, string category)
    //{
    //    if (subjects == null || subjects.Count() == 0) return;

    //    foreach (var subject in subjects)
    //    {
    //        var nonEmptySubjects = subject.Attributes.Where(attr => attr.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)));
    //        if (!nonEmptySubjects.Any()) return;

    //        xmlWriter.WriteStartElement("Subject");
    //        foreach (var attr in nonEmptySubjects)
    //        {
    //            WriteContextAttribute(xmlWriter, attr, XacmlVersion.Version20);
    //        }

    //        xmlWriter.WriteEndElement();
    //    }
    //}

    //private static void WriteResources(XmlWriter writer, IEnumerable<XacmlContextResource> resources, string category)
    //{
    //    if (resources == null || resources.Count() == 0) return;

    //    foreach (var resource in resources)
    //    {
    //        writer.WriteStartElement("Resource");
    //        foreach (var attr in resource.Attributes)
    //            WriteContextAttribute(writer, attr, XacmlVersion.Version20);

    //        writer.WriteEndElement();
    //    }
    //}

    //private static void WriteAction(XmlWriter writer, XacmlContextAction action, string category)
    //{
    //    if (action == null) return;

    //    writer.WriteStartElement("Action");
    //    foreach (var attr in action.Attributes)
    //        WriteContextAttribute(writer, attr, XacmlVersion.Version20);

    //    writer.WriteEndElement();
    //}

    //private static void WriteEnvironment(XmlWriter writer, XacmlContextEnvironment environment, string category)
    //{
    //    if (environment == null) return;

    //    writer.WriteStartElement("Environment");
    //    foreach (var attr in environment.Attributes)
    //        WriteContextAttribute(writer, attr, XacmlVersion.Version20);

    //    writer.WriteEndElement();
    //}

    //private static void WriteContextAttribute(XmlWriter writer, XacmlContextAttribute attr, XacmlVersion xacmlVersion = XacmlVersion.Version30)
    //{
    //    if (attr == null) return;

    //    writer.WriteStartElement("Attribute");
    //    if (xacmlVersion == XacmlVersion.Version30)
    //    {
    //        writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
    //    }

    //    writer.WriteAttributeString("AttributeId", attr.AttributeId.ToString());
    //    writer.WriteAttributeString("DataType", attr.DataType.ToString());


    //    foreach (var val in attr.AttributeValues)
    //    {
    //        writer.WriteStartElement("AttributeValue");
    //        writer.WriteString(val.Value);
    //        writer.WriteEndElement();
    //    }

    //    writer.WriteEndElement();
    //}

    ///// <summary>
    ///// For XACML 2.0
    ///// </summary>
    //private static void WriteAttribute(XmlWriter writer, XacmlAttribute attr)
    //{
    //    if (attr == null) return;

    //    writer.WriteStartElement("Attribute");
    //    writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
    //    writer.WriteAttributeString("AttributeId", attr.AttributeId.ToString());

    //    foreach (var val in attr.AttributeValues)
    //    {
    //        writer.WriteStartElement("AttributeValue");
    //        writer.WriteAttributeString("DataType", val.DataType.ToString());
    //        writer.WriteString(val.Value);
    //        writer.WriteEndElement();
    //    }

    //    writer.WriteEndElement();
    //}
}