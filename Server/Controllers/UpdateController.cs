using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UpdateController : ControllerBase
{
    private readonly ILogger<UpdateController> _logger;
    private readonly IFirmwareRepository _firmwareRepository;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
    private readonly HubConnection _hubConnection;

    public UpdateController(ILogger<UpdateController> logger, HubConnection hubConnection, IFirmwareRepository firmwareRepository, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _logger = logger;
        _hubConnection = hubConnection;
        _firmwareRepository = firmwareRepository;
        _dbContextFactory = dbContextFactory;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Firmware>> GetFirmwares() => Ok(_firmwareRepository.GetAllFirmwares().ToArray());

    [HttpGet("version/current")]
    public ActionResult<Version> GetCurrentVersion() => Ok(_firmwareRepository.GetCurrentVersion());

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

        var mac = Request.Headers["x-ESP8266-STA-MAC"].Single().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
        _logger.LogDebug("ESP8266 [{Mac}] connected", mac);

        var deviceVersion = Request.Headers["x-ESP8266-version"].Single();

        await UpdateSensorInfo(mac, deviceVersion);

        if (!_firmwareRepository.TryGetCurrentVersion(out var currentVeresion))
        {
            _logger.LogWarning($"No current version found");
            return new StatusCodeResult(304);
        }

        if (deviceVersion == currentVeresion.ToString())
        {
            _logger.LogDebug("ESP8266 [{Mac}] nothing new", mac);
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
