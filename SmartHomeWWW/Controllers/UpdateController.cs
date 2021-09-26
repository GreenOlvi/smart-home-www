using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeWWW.Controllers
{
    public class UpdateController : Controller
    {
        public UpdateController(ILogger<UpdateController> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<UpdateController> _logger;

        public IActionResult Index()
        {
            _logger.LogInformation("Update index page");
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
            if (deviceVersion == CurrentVersion)
            {
                _logger.LogInformation($"ESP8266 [{mac}] nothing new");
                return new StatusCodeResult(304);
            }

            if (!System.IO.File.Exists(CurrentFirmware))
            {
                _logger.LogError($"Could not find file with current firmware: {CurrentFirmware}");
                _logger.LogError($"PWD={Directory.GetCurrentDirectory()}");
            }

            return new FileStreamResult(new FileStream(CurrentFirmware, FileMode.Open, FileAccess.Read), "application/octet-stream");
        }

        private const string CurrentFirmware = "firmware/firmware.0.0.4.bin";
        private const string CurrentVersion = "0.0.4";

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
