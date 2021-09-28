using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartHomeWWW.Logic;
using SmartHomeWWW.Logic.Firmware;

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
            ViewBag.Firmwares = _firmwareRepository.GetAllVersions();
            return View();
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
