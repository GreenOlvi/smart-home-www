using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Server.Hubs;

public interface IHubConnection
{
    HubConnectionState State { get; }
    public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);
    public Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default);

    public Task SendUpdateRelayState(Guid id, RelayState state, CancellationToken cancellationToken = default);
    public Task SendRelayDeleted(Guid id, CancellationToken cancellationToken = default);
    public Task SendUpdateSensor(Sensor sensor, CancellationToken cancellationToken = default);
}
