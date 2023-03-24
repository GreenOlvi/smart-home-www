using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHomeWWW.Server.Hubs;

public interface IHubConnection
{
    HubConnectionState State { get; }
    public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);
    public Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default);
}
