using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace XcaXds.Commons.Models.Hl7;

public abstract class Hl7Object
{
    internal class PropertyAndAttribute
    {
        public PropertyInfo Property;
        public Hl7Attribute Hl7Attribute;
    }

    public string Serialize()
    {
        return Serialize(Constants.Hl7.Separator.Hatt);
    }
    private string Serialize(char seperator)
    {
        var stringBuilder = new StringBuilder();

        foreach (var item in GetHl7Properties(this))
        {
            if (item.Property.PropertyType == typeof(Hd))
            {
                var hd = (Hd)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((hd != null ? hd.Serialize(Constants.Hl7.Separator.Amp) : string.Empty) + seperator);
            }
            else if (item.Property.PropertyType == typeof(Sad))
            {
                var sad = (Sad)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((sad != null ? sad.Serialize(Constants.Hl7.Separator.Amp) : string.Empty) + seperator);
            }
            else
            {
                stringBuilder.Append((string)item.Property.GetGetMethod().Invoke(this, null) + seperator);
            }
        }

        var output = Regex.Replace(stringBuilder.ToString(), @"\" + seperator + "+$", string.Empty);
        return output;
    }

    private static IEnumerable<PropertyAndAttribute> GetHl7Properties(Hl7Object instance)
    {
        var output =
            from property in instance.GetType().GetProperties()
            let hl7Attributes = property.GetCustomAttributes(typeof(Hl7Attribute), true)
            where hl7Attributes.Length == 1
            orderby ((Hl7Attribute)hl7Attributes[0]).Sequence
            select new PropertyAndAttribute { Property = property, Hl7Attribute = (Hl7Attribute)hl7Attributes[0] };

        var expectedSequence = 1;
        var propertyAndAttributes = output as PropertyAndAttribute[] ?? output.ToArray();

        foreach (var item in propertyAndAttributes)
        {
            Debug.Assert(item.Hl7Attribute.Sequence == expectedSequence++);
        }

        return propertyAndAttributes;
    }

    public static T Parse<T>(string s) where T : Hl7Object, new()
    {
        return Parse<T>(s, Constants.Hl7.Separator.Hatt);
    }
    private static T Parse<T>(string s, char separator) where T : Hl7Object, new()
    {
        if (s == null)
        {
            return null;
        }

        var output = new T();

        if (separator == Constants.Hl7.Separator.Amp)
        {
            s = HttpUtility.HtmlDecode(s);
        }

        var parts = s.Split(separator);

        foreach (var item in GetHl7Properties(output))
        {
            string value = null;
            if (item.Hl7Attribute.Sequence - 1 <= parts.Length - 1)
            {
                value = parts[item.Hl7Attribute.Sequence - 1];
                if (value == "")
                {
                    value = null;
                }
            }

            object[] obectValue;
#pragma warning disable IDE0045 // Convert to conditional expression
            if (item.Property.PropertyType == typeof(Hd))
            {
                obectValue = new object[] { Parse<Hd>(value, Constants.Hl7.Separator.Amp) };
            }
            else if (item.Property.PropertyType == typeof(Sad))
            {
                obectValue = new object[] { Parse<Sad>(value, Constants.Hl7.Separator.Amp) };
            }
            else
            {
                obectValue = new object[] { value };
            }
#pragma warning restore IDE0045 // Convert to conditional expression

            item.Property.GetSetMethod().Invoke(output, obectValue);
        }
        return output;
    }
}
