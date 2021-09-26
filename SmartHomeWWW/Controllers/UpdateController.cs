using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

            var mac = Request.Headers["x-ESP8266-STA-MAC"];
            if (string.IsNullOrEmpty(mac))
            {
                _logger.LogInformation("Not ESP");
                return new RedirectToActionResult("Index", "Update", null);
            }

            _logger.LogInformation($"ESP8266 [{mac}] connected");
            _logger.LogInformation($"ESP8266 [{mac}] nothing new");

            return new StatusCodeResult(304);
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
