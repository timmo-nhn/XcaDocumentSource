using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Mappers;

/// <summary>
/// Convert external value sets to ComprehensiveCodeSystem for ingesting in ValueSet. 
/// This is used to convert between the value sets used in the XDS specification and the value sets used in the application.
/// </summary>
internal interface IValueSetMapper<T>
{
    public ComprehensiveCodeSystem MapToComprehensiveCodeSystem(T input);
}