using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Domain;
using System.Threading.Tasks;

namespace SmartHomeWWW.Hubs
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
            _logger.LogInformation($"Sent SensorUpdated with {sensor.Mac}");
            await Clients.Others.SendAsync("SensorUpdated", sensor);
        }
    }
}
