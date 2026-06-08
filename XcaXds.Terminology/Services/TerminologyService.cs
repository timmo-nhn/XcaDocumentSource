using Microsoft.Extensions.Logging;
using System.Numerics;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public class TerminologyService
{
    private readonly ILogger<TerminologyService> _logger;

    private Dictionary<string, List<ComprehensiveCodeSystem>> CodeSystems { get; set; } = [];
    
    public TerminologyService(ILogger<TerminologyService> logger)
    {
        _logger = logger;
    }

}