using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Client.Infrastructure;

public interface ISensorsHub
{
    public Task StartIfNotConnectedAsync(CancellationToken cancellationToken = default);

    public IDisposable OnRelayStateUpdated(Action<Guid, RelayState> handler);
    public IDisposable OnSensorUpdated(Action<Sensor> handler);
    public IDisposable OnWeatherUpdated(Action<WeatherReport> handler);

    public void RemoveOnRelayStateUpdated();
    public void RemoveOnSensorUpdated();
    public void RemoveOnWeatherUpdated();
}
