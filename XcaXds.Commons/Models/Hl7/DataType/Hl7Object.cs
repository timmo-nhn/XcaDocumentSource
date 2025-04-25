using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace XcaXds.Commons.Models.Hl7.DataType;

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

    public string Serialize(char separator)
    {
        var stringBuilder = new StringBuilder();

        foreach (var item in GetHl7Properties(this))
        {
            if (item.Property.PropertyType == typeof(HD))
            {
                var hd = (HD)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((hd != null ? hd.Serialize(Constants.Hl7.Separator.Amp) : string.Empty) + separator);
            }
            else if (item.Property.PropertyType == typeof(SAD))
            {
                var sad = (SAD)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((sad != null ? sad.Serialize(Constants.Hl7.Separator.Amp) : string.Empty) + separator);
            }
            else if (item.Property.PropertyType == typeof(CX))
            {
                var cx = (CX)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((cx != null ? cx.Serialize(Constants.Hl7.Separator.Hatt) : string.Empty) + separator);
            }
            else if (item.Property.PropertyType == typeof(XPN))
            {
                var xpn = (XPN)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((xpn != null ? xpn.Serialize(Constants.Hl7.Separator.Hatt) : string.Empty) + separator);
            }
            else if (item.Property.PropertyType == typeof(DateTime))
            {
                var dt = (DateTime)item.Property.GetGetMethod().Invoke(this, null);
                stringBuilder.Append((dt != DateTime.MinValue ? dt.ToString(Constants.Hl7.Dtm.DtmYmdFormat) : string.Empty) + separator);
            }
            else
            {
                stringBuilder.Append((string)item.Property.GetGetMethod().Invoke(this, null) + separator);
            }
        }

        var output = Regex.Replace(stringBuilder.ToString(), @"\" + separator + "+$", string.Empty);
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

    public static T Parse<T>(string s, char separator) where T : Hl7Object, new()
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
            if (value == null) continue;

#pragma warning disable IDE0045 // Convert to conditional expression
            if (item.Property.PropertyType == typeof(HD))
            {
                obectValue = new object[] { Parse<HD>(value, Constants.Hl7.Separator.Amp) };
            }
            else if (item.Property.PropertyType == typeof(SAD))
            {
                obectValue = new object[] { Parse<SAD>(value, Constants.Hl7.Separator.Amp) };
            }
            else if (item.Property.PropertyType == typeof(CX))
            {
                obectValue = new object[] { Parse<CX>(value, Constants.Hl7.Separator.Hatt) };
            }
            else if (item.Property.PropertyType == typeof(XPN))
            {
                obectValue = new object[] { Parse<XPN>(value, Constants.Hl7.Separator.Hatt) };
            }
            else if (item.Property.PropertyType == typeof(DateTime) )
            {
                obectValue = new object[] { DateTime.ParseExact(value, Constants.Hl7.Dtm.CdaFormats, CultureInfo.InvariantCulture) };
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
