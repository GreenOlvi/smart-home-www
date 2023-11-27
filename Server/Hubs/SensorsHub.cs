using Microsoft.AspNetCore.SignalR;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Server.Hubs;

public class SensorsHub(ILogger<SensorsHub> logger) : Hub
{
    public const string RelativePath = "/sensorshub";
    private readonly ILogger<SensorsHub> _logger = logger;

    public async Task UpdateSensor(Sensor sensor)
    {
        _logger.LogDebug("Sent SensorUpdated with {Mac}", sensor.Mac);
        await Clients.Others.SendAsync("SensorUpdated", sensor);
    }

    public async Task UpdateWeather(WeatherReport weather)
    {
        _logger.LogDebug("Updated current weather at {Dt}.", weather.Current.Timestamp.ToLocalTime().ToString());
        await Clients.Others.SendAsync("WeatherUpdated", weather);
    }

    public async Task UpdateRelayState(string deviceId, RelayState state)
    {
        _logger.LogDebug("Updated relay {Device} state to '{State}'", deviceId, state.ToString());
        await Clients.Others.SendAsync("RelayStateUpdated", deviceId, state);
    }

    public Task RelayDeleted(Guid id)
    {
        _logger.LogDebug("Relay {Id} deleted", id);
        return Clients.Others.SendAsync("RelayDeleted", id);
    }
}
