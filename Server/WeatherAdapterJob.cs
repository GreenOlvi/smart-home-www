using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server;

public sealed class WeatherAdapterJob : IOrchestratorJob, IMessageHandler<WeatherUpdatedEvent>
{
    private readonly IMessageBus _bus;
    private readonly HubConnection _hubConnection;

    public WeatherAdapterJob(IMessageBus bus, HubConnection hubConnection)
    {
        _bus = bus;
        _hubConnection = hubConnection;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task Handle(WeatherUpdatedEvent message)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }

        await _hubConnection.SendAsync("UpdateWeather", message.Weather);
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _bus.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _bus.Unsubscribe(this);
        return Task.CompletedTask;
    }
}
