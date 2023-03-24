using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Server.Hubs;

public class HubConnectionWrapper : IHubConnection
{
    private readonly HubConnection _hubConnection;

    public HubConnectionWrapper(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

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

    private Task StartAsync(CancellationToken cancellationToken = default) => _hubConnection.StartAsync(cancellationToken);
}
