using System.ComponentModel.DataAnnotations;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class StringArrayConstraintsAttribute : ValidationAttribute
{
    public int MaxItems { get; }
    public int MaxStringLength { get; }

    public StringArrayConstraintsAttribute(int maxItems, int maxStringLength)
    {
        MaxItems = maxItems;
        MaxStringLength = maxStringLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string[] arr)
            return ValidationResult.Success;

        if (arr.Length > MaxItems)
        {
            return new ValidationResult(
                $"Maximum number of items is {MaxItems}.");
        }

        foreach (var item in arr)
        {
            if (item != null && item.Length > MaxStringLength)
            {
                var typeName = context.ObjectInstance.ToString();

                var memberNames = new[]
                {
                    "MemberName: " + context.MemberName,
                    "Type: " + typeName,
                    "Value: "+item
                };

                return new ValidationResult(
                    $"Each string must be at most {MaxStringLength} characters.",memberNames);
            }
        }

        return ValidationResult.Success;
    }
}