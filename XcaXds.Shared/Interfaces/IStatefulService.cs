using XcaXds.Shared.Enums;

namespace XcaXds.Shared.Interfaces;

public interface IStatefulService
{
    ServiceState ServiceStatus { get; }
    ServiceState GetServiceState();
    void SetServiceState(ServiceState newState);
    Task InitializeAsync(CancellationToken cancellationToken);
}
