namespace XcaXds.Commons.Models.Custom;

public class InterfaceImplementation
{
    public InterfaceImplementation(string serviceType, string implementationType)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
    }

    public string ImplementationType { get; set; }
    public string ServiceType { get; set; }
}