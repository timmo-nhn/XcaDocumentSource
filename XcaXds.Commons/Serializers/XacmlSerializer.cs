using System.Text;
using System.Xml;
using Abc.Xacml.Context;
using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Serializers;
public static class XacmlSerializer
{
    public static string? SerializeRequestToXml(XacmlContextRequest? request)
    {
        if (request == null) return string.Empty;

        var settings = new XmlWriterSettings()
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false)
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();

        if (request.Resources.Count == 0)
        {
            // XACML 3.0
            xmlWriter.WriteStartElement("Request", Constants.Xacml.Namespace.WD17);
            xmlWriter.WriteAttributeString("ReturnPolicyIdList", request.ReturnPolicyIdList.ToString().ToLower());
            xmlWriter.WriteAttributeString("CombinedDecision", request.CombinedDecision.ToString().ToLower());
        }
        else
        {
            // XACML 2.0
            xmlWriter.WriteStartElement("Request", Constants.Xacml.Namespace.Context_OS);
        }


        // Request.Subjects, Request.Resources etc. is essentially a XACML Version check,
        // V2 is populated, while V3 has everything in resource.Attributes

        if (request.Subjects.Count == 0)
        {
            var subjectAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("subject")));
            WriteCategoryAttributes(xmlWriter, subjectAttributes, Constants.Xacml.Category.V30_Subject);
        }
        else
        {
            WriteSubject(xmlWriter, request.Subjects, Constants.Xacml.Category.V20_Subject);
        }

        if (request.Resources.Count == 0)
        {
            var resourceAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("resource-id")));
            WriteCategoryAttributes(xmlWriter, resourceAttributes, Constants.Xacml.Category.V30_Resource);
        }
        else
        {
            WriteResources(xmlWriter, request.Resources, Constants.Xacml.Category.V20_Resource);
        }

        if (request.Action == null)
        {
            var actionAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("action-id")));
            WriteCategoryAttributes(xmlWriter, actionAttributes, Constants.Xacml.Category.V30_Action);
        }
        else
        {
            WriteAction(xmlWriter, request.Action, Constants.Xacml.Category.V20_Action);
        }

        if (request.Environment == null)
        {
            var environmentAttributes = request.Attributes.Where(xatt => xatt.Attributes.Any(xid => xid.AttributeId.AbsolutePath.Contains("environment")));
            WriteCategoryAttributes(xmlWriter, environmentAttributes, Constants.Xacml.Category.V30_Environment);
        }
        else
        {
            WriteEnvironment(xmlWriter, request.Environment, Constants.Xacml.Category.V20_Environment);
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();

        var serializedRequest = stringWriter.ToString();
        serializedRequest = serializedRequest.Replace("&amp;amp;", "&amp;");

        return serializedRequest;
    }

    /// <summary>
    /// For XACML 3.0
    /// </summary>
    private static void WriteCategoryAttributes(XmlWriter xmlWriter, IEnumerable<XacmlContextAttributes> categoryAttributes, string category)
    {
        if (categoryAttributes.Count() == 0) return;

        foreach (var attribute in categoryAttributes)
        {
            xmlWriter.WriteStartElement("Attributes");
            xmlWriter.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
            xmlWriter.WriteAttributeString("Category", category);

            foreach (var attr in attribute.Attributes)
            {
                WriteAttribute(xmlWriter, attr);
            }

            xmlWriter.WriteEndElement();
        }
    }

    private static void WriteSubject(XmlWriter xmlWriter, ICollection<XacmlContextSubject> subjects, string category)
    {
        if (subjects == null || subjects.Count() == 0) return;

        foreach (var subject in subjects)
        {
            var nonEmptySubjects = subject.Attributes.Where(attr => attr.AttributeValues.All(av => !string.IsNullOrWhiteSpace(av.Value)));
            if (!nonEmptySubjects.Any()) return;

            xmlWriter.WriteStartElement("Subject");
            foreach (var attr in nonEmptySubjects)
            {
                WriteContextAttribute(xmlWriter, attr, XacmlVersion.Version20);
            }

            xmlWriter.WriteEndElement();
        }
    }

    private static void WriteResources(XmlWriter writer, IEnumerable<XacmlContextResource> resources, string category)
    {
        if (resources == null || resources.Count() == 0) return;

        foreach (var resource in resources)
        {
            writer.WriteStartElement("Resource");
            foreach (var attr in resource.Attributes)
                WriteContextAttribute(writer, attr, XacmlVersion.Version20);

            writer.WriteEndElement();
        }
    }

    private static void WriteAction(XmlWriter writer, XacmlContextAction action, string category)
    {
        if (action == null) return;

        writer.WriteStartElement("Action");
        foreach (var attr in action.Attributes)
            WriteContextAttribute(writer, attr, XacmlVersion.Version20);

        writer.WriteEndElement();
    }

    private static void WriteEnvironment(XmlWriter writer, XacmlContextEnvironment environment, string category)
    {
        if (environment == null) return;

        writer.WriteStartElement("Environment");
        foreach (var attr in environment.Attributes)
            WriteContextAttribute(writer, attr, XacmlVersion.Version20);

        writer.WriteEndElement();
    }

    private static void WriteContextAttribute(XmlWriter writer, XacmlContextAttribute attr, XacmlVersion xacmlVersion = XacmlVersion.Version30)
    {
        if (attr == null) return;

        writer.WriteStartElement("Attribute");
        if (xacmlVersion == XacmlVersion.Version30)
        {
            writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
        }
        
        writer.WriteAttributeString("AttributeId", attr.AttributeId.ToString());
        writer.WriteAttributeString("DataType", attr.DataType.ToString());


        foreach (var val in attr.AttributeValues)
        {
            writer.WriteStartElement("AttributeValue");
            writer.WriteString(val.Value);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// For XACML 2.0
    /// </summary>
    private static void WriteAttribute(XmlWriter writer, XacmlAttribute attr)
    {
        if (attr == null) return;

        writer.WriteStartElement("Attribute");
        writer.WriteAttributeString("IncludeInResult", bool.FalseString.ToLower());
        writer.WriteAttributeString("AttributeId", attr.AttributeId.ToString());

        foreach (var val in attr.AttributeValues)
        {
            writer.WriteStartElement("AttributeValue");
            writer.WriteAttributeString("DataType", val.DataType.ToString());
            writer.WriteString(val.Value);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }
}