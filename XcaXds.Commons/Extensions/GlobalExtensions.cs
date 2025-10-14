using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Extensions;

public static class GlobalExtensions
{
    public static bool TryThis(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}