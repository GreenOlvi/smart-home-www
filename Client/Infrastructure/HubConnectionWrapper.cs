using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Client.Infrastructure;

public class HubConnectionWrapper(HubConnection hubConnection) : IHubConnection, ISensorsHub
{
    private const string RelayStateUpdated = "RelayStateUpdated";
    private const string RelayDeleted = "RelayDeleted";
    private const string SensorUpdated = "SensorUpdated";
    private const string WeatherUpdated = "WeatherUpdated";

    private readonly HubConnection _hubConnection = hubConnection;

    public HubConnectionState State => _hubConnection.State;

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _hubConnection.StartAsync(cancellationToken);

    public Task StartIfNotConnectedAsync(CancellationToken cancellationToken = default) =>
        State == HubConnectionState.Disconnected
        ? _hubConnection.StartAsync(cancellationToken)
        : Task.CompletedTask;

    public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default) =>
        _hubConnection.SendAsync(methodName, arg1, cancellationToken);

    public Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default) =>
        _hubConnection.SendAsync(methodName, arg1, arg2, cancellationToken);

    public IDisposable On<T>(string methodName, Action<T> handler) => _hubConnection.On(methodName, handler);
    public IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler) => _hubConnection.On(methodName, handler);

    public void Remove(string methodName) => _hubConnection.Remove(methodName);

    public IDisposable OnRelayStateUpdated(Action<Guid, RelayState> handler) => _hubConnection.On(RelayStateUpdated, handler);
    public IDisposable OnRelayDeleted(Action<Guid> handler) => _hubConnection.On(RelayDeleted, handler);
    public IDisposable OnSensorUpdated(Action<Sensor> handler) => _hubConnection.On(SensorUpdated, handler);
    public IDisposable OnWeatherUpdated(Action<WeatherReport> handler) => _hubConnection.On(WeatherUpdated, handler);

    public void RemoveOnRelayStateUpdated() => _hubConnection.Remove(RelayStateUpdated);
    public void RemoveOnRelayDeleted() => _hubConnection.Remove(RelayDeleted);
    public void RemoveOnSensorUpdated() => _hubConnection.Remove(SensorUpdated);
    public void RemoveOnWeatherUpdated() => _hubConnection.Remove(WeatherUpdated);
}
