using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHomeWWW.Server.Hubs;

public class HubConnectionWrapper : IHubConnection
{
    private readonly HubConnection _hubConnection;

    public HubConnectionWrapper(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    public HubConnectionState State => _hubConnection.State;

    public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default) =>
        _hubConnection.SendAsync(methodName, arg1, cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken = default) => _hubConnection.StartAsync(cancellationToken);
}
