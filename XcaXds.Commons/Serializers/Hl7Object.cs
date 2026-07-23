using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Shared;

namespace XcaXds.Commons.Serializers;

public abstract class Hl7Object
{
    private const char PipeSeparator = '|';
    private static readonly char[] SeparatorCandidates = [Constants.Hl7.Separator.Caret, Constants.Hl7.Separator.Ampersand, PipeSeparator];

    internal class PropertyAndAttribute
    {
        public PropertyInfo? Property;
        public Hl7Attribute? Hl7Attribute;
    }

    public string? Serialize()
    {
        return Serialize(Constants.Hl7.Separator.Caret);
    }

    public string? Serialize(char separator)
    {
        return Serialize(separator, []);
    }

    private string? Serialize(char separator, IReadOnlyCollection<char> ancestorSeparators)
    {
        var stringBuilder = new StringBuilder();
        var nestedAncestors = CreateNestedAncestors(ancestorSeparators, separator);

        foreach (var item in GetHl7Properties(this))
        {
            if (item.Property?.PropertyType != null && typeof(Hl7Object).IsAssignableFrom(item.Property.PropertyType))
            {
                var nestedHl7Object = (Hl7Object?)item.Property.GetGetMethod()?.Invoke(this, null);
                var nestedSeparator = GetNestedSeparator(separator, nestedAncestors);
                stringBuilder.Append((nestedHl7Object != null ? nestedHl7Object.Serialize(nestedSeparator, nestedAncestors) : string.Empty) + separator);
            }
            else if (item.Property?.PropertyType == typeof(DateTime))
            {
                var dt = (DateTime)(item.Property.GetGetMethod()?.Invoke(this, null) ?? DateTime.MinValue);
                stringBuilder.Append((dt != DateTime.MinValue ? dt.ToString(Constants.Hl7.Dtm.DtmYmdFormat) : string.Empty) + separator);
            }
            else
            {
                stringBuilder.Append((string?)item.Property?.GetGetMethod()?.Invoke(this, null) + separator);
            }
        }

        var output = Regex.Replace(stringBuilder.ToString(), @"\" + separator + "+$", string.Empty);
        return output;
    }

    public bool Equals(Hl7Object? hl7Object)
    {
        return this.Serialize() == hl7Object?.Serialize();
    }

    private static PropertyAndAttribute[] GetHl7Properties(Hl7Object instance)
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
            Debug.Assert(item.Hl7Attribute?.Sequence == expectedSequence++);
        }

        return propertyAndAttributes;
    }

    public static T? Parse<T>(string? s) where T : Hl7Object, new()
    {
        return Parse<T>(s, Constants.Hl7.Separator.Caret);
    }

    public static T? Parse<T>(string? s, char separator) where T : Hl7Object, new()
    {
        return Parse(typeof(T), s, separator, []) as T;
    }

    private static Hl7Object? Parse(Type hl7Type, string? s, char separator, IReadOnlyCollection<char> ancestorSeparators)
    {
        if (s == null)
        {
            return null;
        }

        var output = Activator.CreateInstance(hl7Type) as Hl7Object;
        ArgumentNullException.ThrowIfNull(output);

        if (separator == Constants.Hl7.Separator.Ampersand)
        {
            s = HttpUtility.HtmlDecode(s);
        }

        var parts = s.Split(separator);
        var nestedAncestors = CreateNestedAncestors(ancestorSeparators, separator);

        foreach (var item in GetHl7Properties(output))
        {
            if (item.Property == null)
            {
                continue;
            }

            string? value = null;
            if (item.Hl7Attribute?.Sequence - 1 <= parts.Length - 1)
            {
                value = parts[(item.Hl7Attribute?.Sequence - 1) ?? 0];
                if (value == "")
                {
                    value = null;
                }
            }

            if (value == null)
            {
                continue;
            }

            object?[] objectValue;
            if (typeof(Hl7Object).IsAssignableFrom(item.Property.PropertyType))
            {
                var nestedSeparator = GetNestedSeparator(separator, nestedAncestors);
                objectValue = [Parse(item.Property.PropertyType, value, nestedSeparator, nestedAncestors)];
            }
            else if (item.Property.PropertyType == typeof(DateTime))
            {
                objectValue = [DateTime.ParseExact(value, Constants.Hl7.Dtm.AllFormats, CultureInfo.InvariantCulture)];
            }
            else
            {
                objectValue = [value];
            }

            item.Property.GetSetMethod()?.Invoke(output, objectValue);
        }

        return output;
    }

    private static IReadOnlyCollection<char> CreateNestedAncestors(IReadOnlyCollection<char> ancestorSeparators, char currentSeparator)
    {
        var usedSeparators = new HashSet<char>(ancestorSeparators)
        {
            currentSeparator
        };
        return usedSeparators;
    }

    private static char GetNestedSeparator(char currentSeparator, IReadOnlyCollection<char> usedSeparators)
    {
        var preferredNestedSeparator = currentSeparator == Constants.Hl7.Separator.Ampersand
            ? Constants.Hl7.Separator.Caret
            : Constants.Hl7.Separator.Ampersand;

        if (!usedSeparators.Contains(preferredNestedSeparator))
        {
            return preferredNestedSeparator;
        }

        foreach (var candidate in SeparatorCandidates)
        {
            if (!usedSeparators.Contains(candidate))
            {
                return candidate;
            }
        }

        return preferredNestedSeparator;
    }
}
