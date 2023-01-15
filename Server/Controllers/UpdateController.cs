using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.ViewModel;
using SmartHomeWWW.Server.Hubs;
using System.Text;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UpdateController : ControllerBase
{
    private readonly ILogger<UpdateController> _logger;
    private readonly IFirmwareRepository _firmwareRepository;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
    private readonly IHubConnection _hubConnection;

    public UpdateController(ILogger<UpdateController> logger, IHubConnection hubConnection, IFirmwareRepository firmwareRepository, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _logger = logger;
        _hubConnection = hubConnection;
        _firmwareRepository = firmwareRepository;
        _dbContextFactory = dbContextFactory;
    }

    [HttpGet]
    public ActionResult<IEnumerable<IFirmware>> GetFirmwares() =>
        Ok(_firmwareRepository.GetAllFirmwares().Select(FirmwareViewModel.From).ToArray());

    [HttpGet("version/current")]
    public ActionResult<IDictionary<UpdateChannel, FirmwareVersion>> GetCurrentVersion()
    {
        var versions = Enum.GetValues<UpdateChannel>()
            .Select(c => (c, _firmwareRepository.GetCurrentFirmware(c)))
            .Where(cf => cf.Item2 is not null)
            .Select(cf => (cf.c, cf.Item2?.Version))
            .ToDictionary(cf => cf.c, cf => cf.Version);
        return Ok(versions);
    }

    [HttpGet("/Update/firmware.bin")]
    public async Task<IActionResult> Firmware()
    {
        _logger.LogTrace("{Headers}", DumpHeaders(Request.Headers));

        var userAgent = Request.Headers["User-Agent"];
        if (userAgent != "ESP8266-http-Update")
        {
            _logger.LogDebug("Not ESP");
            return new RedirectResult("/");
        }

        var mac = Request.Headers["x-ESP8266-STA-MAC"].Single()?.ToUpperInvariant();
        if (mac is null)
        {
            _logger.LogWarning("Missing x-ESP8266-STA-MAC header");
            return new RedirectResult("/");
        }
        _logger.LogDebug("ESP8266 [{Mac}] connected", mac);

        var deviceVersion = Request.Headers["x-ESP8266-version"].Single();
        if (deviceVersion is null)
        {
            _logger.LogWarning("Missing x-ESP8266-version header");
            return new RedirectResult("/");
        }

        using var db = _dbContextFactory.CreateDbContext();
        var sensor = await GetAndUpdateSensor(db, mac, deviceVersion);
        await db.SaveChangesAsync();

        await NotifySensorsHub(sensor);

        var channel = GetSensorUpdateChannel(sensor);
        _logger.LogDebug("Sensor uses [{Channel}] update channel", channel);

        var firmware = _firmwareRepository.GetCurrentFirmware(channel);
        if (firmware is null)
        {
            _logger.LogInformation("No current {Channel} version found", channel);
            return new StatusCodeResult(304);
        }

        if (!FirmwareVersion.TryParse(deviceVersion, out var devVersion))
        {
            _logger.LogWarning("Could not parse device firmware version '{DeviceVersion}'", deviceVersion);
            return new StatusCodeResult(304);
        }

        if (devVersion.Prefix >= firmware.Version.Prefix)
        {
            _logger.LogDebug("ESP8266 [{Mac}] nothing new", mac);
            return new StatusCodeResult(304);
        }

        var filename = $"firmware.{firmware.Version}.bin";
        _logger.LogDebug("Sending '{Filename}' to the device", filename);
        return new FileStreamResult(firmware.GetData(), "application/octet-stream")
        {
            FileDownloadName = filename,
        };
    }

    private static async Task<Sensor> GetAndUpdateSensor(SmartHomeDbContext db, string mac, string firmwareVersion)
    {
        var sensor = await db.Sensors
            .FirstOrDefaultAsync(s => s.Mac == mac);

        if (sensor is null)
        {
            sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                ChipType = "ESP8266",
            };

            db.Sensors.Add(sensor);
        }

        sensor.FirmwareVersion = firmwareVersion;
        sensor.LastContact = DateTime.UtcNow;

        return sensor;
    }

    private static UpdateChannel GetSensorUpdateChannel(Sensor sensor) =>
        sensor.UpdateChannel switch
        {
            "alpha" => UpdateChannel.Alpha,
            "beta" => UpdateChannel.Beta,
            _ => UpdateChannel.Stable,
        };

    private async Task NotifySensorsHub(Sensor sensor)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
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
            sb.AppendJoin("; ", header.Value.AsEnumerable());
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
