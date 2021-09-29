using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Firmwares;
using SmartHomeWWW.Models;
using System;
using System.Linq;
using System.Text;

namespace SmartHomeWWW.Controllers
{
    public class UpdateController : Controller
    {
        public UpdateController(ILogger<UpdateController> logger, IFirmwareRepository firmwareRepository)
        {
            _logger = logger;
            _firmwareRepository = firmwareRepository;
        }

        private readonly ILogger<UpdateController> _logger;
        private readonly IFirmwareRepository _firmwareRepository;

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
        public IActionResult Firmware()
        {
            _logger.LogInformation(DumpHeaders(Request.Headers));

            var userAgent = Request.Headers["User-Agent"];
            if (userAgent != "ESP8266-http-Update")
            {
                _logger.LogInformation("Not ESP");
                return new RedirectToActionResult("Index", "Update", null);
            }

            var mac = Request.Headers["x-ESP8266-STA-MAC"];
            _logger.LogInformation($"ESP8266 [{mac}] connected");

            var deviceVersion = Request.Headers["x-ESP8266-version"];
            if (deviceVersion == _firmwareRepository.GetCurrentVersion())
            {
                _logger.LogInformation($"ESP8266 [{mac}] nothing new");
                return new StatusCodeResult(304);
            }

            return new FileStreamResult(_firmwareRepository.GetCurrentFirmware(), "application/octet-stream");
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
