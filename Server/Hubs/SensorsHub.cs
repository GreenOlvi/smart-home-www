using Microsoft.AspNetCore.SignalR;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;

namespace SmartHomeWWW.Server.Hubs
{
    public class SensorsHub : Hub
    {
        public SensorsHub(ILogger<SensorsHub> logger)
        {
            _logger = logger;
        }

        public const string RelativePath = "/sensorshub";
        private readonly ILogger<SensorsHub> _logger;

        public async Task UpdateSensor(Sensor sensor)
        {
            _logger.LogDebug("Sent SensorUpdated with {mac}", sensor.Mac);
            await Clients.Others.SendAsync("SensorUpdated", sensor);
        }

        public async Task UpdateWeather(WeatherReport weather)
        {
            _logger.LogDebug("Updated current weather at {dt}.", weather.Current.Timestamp.ToLocalTime().ToString());
            await Clients.Others.SendAsync("WeatherUpdated", weather);
        }
    }
}
