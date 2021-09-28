using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartHomeWWW.Logic.Firmware
{
    public class DiskFirmwareRepository : IFirmwareRepository
    {
        public DiskFirmwareRepository(ILogger<DiskFirmwareRepository> logger, string firmwarePath)
        {
            _logger = logger;
            _firmwarePath = firmwarePath;
        }

        private readonly ILogger<DiskFirmwareRepository> _logger;
        private readonly string _firmwarePath;

        public IEnumerable<Version> GetAllVersions()
        {
            _logger.LogInformation($"Checking [{_firmwarePath}]");
            if (!Directory.Exists(_firmwarePath))
            {
                _logger.LogInformation($"Path [{Path.GetFullPath(_firmwarePath)}] does not exist");
                return Enumerable.Empty<Version>();
            }

            return Directory.GetFiles(_firmwarePath)
                .Select(n => Path.GetFileName(n))
                .Select(n =>
                {
                    _logger.LogInformation($"Found: [{n}]");
                    var c = FirmwareUtils.TryExtractVersionFromFileName(n, out var version);
                    return (Success: c, Version: version);
                })
                .Where(p => p.Success)
                .Select(p => p.Version);
        }

        public Version GetCurrentVersion() => GetAllVersions().Max();

        public Stream GetCurrentFirmware()
        {
            var version = GetCurrentVersion();
            var filePath = Path.Combine(_firmwarePath, $"firmware.{version}.bin");
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}
