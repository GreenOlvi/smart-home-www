using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHomeWWW.Client.Infrastructure;

public interface IHubConnection
{
    HubConnectionState State { get; }
    public Task StartAsync(CancellationToken cancellationToken = default);

    public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);
    public Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default);

    public IDisposable On<T>(string methodName, Action<T> handler);
    public IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler);

    public void Remove(string methodName);
}
