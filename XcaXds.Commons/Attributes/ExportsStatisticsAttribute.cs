namespace XcaXds.Commons.Attributes;

/// <summary>
/// Declares endpoint to export statistics about the request and response to the StatisticsProcessorService, which may be used for monitoring and analytics.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ExportsStatisticsAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
}