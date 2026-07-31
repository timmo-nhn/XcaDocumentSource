using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.WebService.Extensions;

public class ImplementationInformerService
{
    private readonly ILogger<ImplementationInformerService> _logger;

    private static InterfaceImplementation[] _implementations = [];

    public ImplementationInformerService(ILogger<ImplementationInformerService> logger)
    {
        _logger = logger;
    }

    public void Initialize(WebApplicationBuilder builder)
    {
        var types = new Type[]
        {
            typeof(IRegistry),
            typeof(IRepository),
            typeof(IPolicyRepository),
        };

        var implementations = new List<InterfaceImplementation>();

        foreach (var type in types)
        {
            var diDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == type);
            if (diDescriptor?.ImplementationType != null)
            {
                _logger.LogInformation("Active {interface} Implementation: {implementation}", diDescriptor.ServiceType.Name, diDescriptor.ImplementationType);
                implementations.Add(new InterfaceImplementation(diDescriptor.ServiceType.Name, diDescriptor.ImplementationType.Name));
            }
        }


        _implementations = implementations.ToArray();
    }

    public InterfaceImplementation[] GetImplementations()
    {
        return _implementations;
    }
}