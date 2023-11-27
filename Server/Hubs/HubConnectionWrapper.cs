using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Server.Hubs;

public class HubConnectionWrapper(HubConnection hubConnection) : IHubConnection
{
    private readonly HubConnection _hubConnection = hubConnection;

    public HubConnectionState State => _hubConnection.State;

    public async Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default)
    {
        if (State == HubConnectionState.Disconnected)
        {
            await StartAsync(cancellationToken);
        }

        await _hubConnection.SendAsync(methodName, arg1, cancellationToken);
    }

    public async Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default)
    {
        if (State == HubConnectionState.Disconnected)
        {
            await StartAsync(cancellationToken);
        }

        await _hubConnection.SendAsync(methodName, arg1, arg2, cancellationToken);
    }

    public Task SendUpdateRelayState(Guid id, RelayState state, CancellationToken cancellationToken = default) =>
        SendAsync(nameof(SensorsHub.UpdateRelayState), id, state, cancellationToken);

    public Task SendRelayDeleted(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(nameof(SensorsHub.RelayDeleted), id, cancellationToken);

    public Task SendUpdateSensor(Sensor sensor, CancellationToken cancellationToken = default) =>
        SendAsync(nameof(SensorsHub.UpdateSensor), sensor, cancellationToken);

    private Task StartAsync(CancellationToken cancellationToken = default) => _hubConnection.StartAsync(cancellationToken);
}
