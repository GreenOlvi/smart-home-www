﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Domain;
using SmartHomeCore.Firmwares;
using SmartHomeCore.Infrastructure;
using SmartHomeWWW.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeWWW.Controllers
{
    public class UpdateController : Controller
    {
        public UpdateController(ILogger<UpdateController> logger, IFirmwareRepository firmwareRepository, SmartHomeDbContext dbContext)
        {
            _logger = logger;
            _firmwareRepository = firmwareRepository;
            _dbContext = dbContext;
        }

        private readonly ILogger<UpdateController> _logger;
        private readonly IFirmwareRepository _firmwareRepository;
        private readonly SmartHomeDbContext _dbContext;

        public IActionResult Index()
        {
            var list = new FirmwareListViewModel
            {
                Firmwares = Array.AsReadOnly(_firmwareRepository.GetAllFirmwares()
                    .Select(f => FirmwareViewModel.FromFirmware(f))
                    .ToArray()),
            };
            return View(list);
        }

        [HttpGet("/{controller}/firmware.bin")]
        public async Task<IActionResult> Firmware()
        {
            _logger.LogInformation(DumpHeaders(Request.Headers));

            var userAgent = Request.Headers["User-Agent"];
            if (userAgent != "ESP8266-http-Update")
            {
                _logger.LogInformation("Not ESP");
                return new RedirectToActionResult("Index", "Update", null);
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
}
