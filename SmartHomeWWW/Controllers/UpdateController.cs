using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Domain;
using SmartHomeCore.Firmwares;
using SmartHomeCore.Infrastructure;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeWWW.Controllers
{
    public class UpdateController : Controller, IAsyncDisposable
    {
        private readonly ILogger<UpdateController> _logger;
        private readonly IFirmwareRepository _firmwareRepository;
        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
        private HubConnection _hubConnection = null;

        public UpdateController(ILogger<UpdateController> logger, IFirmwareRepository firmwareRepository, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _logger = logger;
            _firmwareRepository = firmwareRepository;
            _dbContextFactory = dbContextFactory;
        }

        [HttpGet("/{controller}/firmware.bin")]
        public async Task<IActionResult> Firmware()
        {
            _logger.LogInformation(DumpHeaders(Request.Headers));

            var userAgent = Request.Headers["User-Agent"];
            if (userAgent != "ESP8266-http-Update")
            {
                _logger.LogInformation("Not ESP");
                return new RedirectResult("/");
            }

            var mac = Request.Headers["x-ESP8266-STA-MAC"].Single().ToUpper();
            _logger.LogInformation($"ESP8266 [{mac}] connected");

            var deviceVersion = Request.Headers["x-ESP8266-version"].Single();

            await UpdateSensorInfo(mac, deviceVersion);

            if (!_firmwareRepository.TryGetCurrentVersion(out var currentVeresion))
            {
                _logger.LogWarning($"No current version found");
                return new StatusCodeResult(304);
            }

            if (deviceVersion == currentVeresion.ToString())
            {
                _logger.LogInformation($"ESP8266 [{mac}] nothing new");
                return new StatusCodeResult(304);
            }

            return new FileStreamResult(_firmwareRepository.GetCurrentFirmware(), "application/octet-stream");
        }

        private async Task UpdateSensorInfo(string mac, string firmwareVersion)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();

            var sensor = await _dbContext.Sensors
                .FirstOrDefaultAsync(s => s.Mac == mac);

            if (sensor is null)
            {
                sensor = new Sensor
                {
                    Id = Guid.NewGuid(),
                    Mac = mac,
                };

                _dbContext.Sensors.Add(sensor);
            }

            sensor.ChipType = "ESP8266";
            sensor.LastContact = DateTime.UtcNow;
            sensor.FirmwareVersion = firmwareVersion;

            await _dbContext.SaveChangesAsync();

            await NotifySensorsHub(sensor);
        }

        private async Task NotifySensorsHub(Sensor sensor)
        {
            if (_hubConnection is null)
            {
                var sensorHubUrl = $"{Request.Scheme}://{Request.Host}{Hubs.SensorsHub.RelativePath}";

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(sensorHubUrl)
                    .Build();
                await _hubConnection.StartAsync();
            }

            await _hubConnection.SendAsync("UpdateSensor", sensor);
        }

        private static string DumpHeaders(IHeaderDictionary headers)
        {
            var sb = new StringBuilder();
            foreach (var header in headers)
            {
                sb.Append($"{header.Key}: ");
                sb.AppendJoin("; ", header.Value);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
