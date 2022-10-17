using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHomeWWW.Server.Hubs;

public interface IHubConnection
{
    HubConnectionState State { get; }
    public Task StartAsync(CancellationToken cancellationToken = default);
    public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);
}
