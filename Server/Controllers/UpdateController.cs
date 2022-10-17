using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Hubs;

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
    public ActionResult<IEnumerable<IFirmware>> GetFirmwares() => Ok(_firmwareRepository.GetAllFirmwares().ToArray());

    [HttpGet("version/current")]
    public ActionResult<FirmwareVersion?> GetCurrentVersion() => Ok(_firmwareRepository.GetCurrentFirmware()?.Version);

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

        var mac = Request.Headers["x-ESP8266-STA-MAC"].Single().ToUpperInvariant();
        _logger.LogDebug("ESP8266 [{Mac}] connected", mac);

        var deviceVersion = Request.Headers["x-ESP8266-version"].Single();

        using var db = _dbContextFactory.CreateDbContext();
        var sensor = await GetAndUpdateSensor(db, mac, deviceVersion);
        await db.SaveChangesAsync();

        await NotifySensorsHub(sensor);

        var channel = GetSensorUpdateChannel(sensor);
        _logger.LogDebug("Sensor uses [{Channel}] update channel", channel);

        var firmware = _firmwareRepository.GetCurrentFirmware(channel);
        if (firmware is null)
        {
            _logger.LogWarning("No current {Channel} version found", channel);
            return new StatusCodeResult(304);
        }

        if (deviceVersion == firmware.Version.ToString())
        {
            _logger.LogDebug("ESP8266 [{Mac}] nothing new", mac);
            return new StatusCodeResult(304);
        }

        var filename = $"firmware.{firmware.Version}.bin";
        _logger.LogDebug("Sending '{Filename}' to the device", filename);
        return new FileStreamResult(firmware.Data, "application/octet-stream")
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
            sb.AppendJoin("; ", header.Value);
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
