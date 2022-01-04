using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SmartHomeWWW.Core.Domain;
using SmartHomeWWW.Core.Utils;

namespace SmartHomeWWW.Core.Firmwares
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

        public IEnumerable<Firmware> GetAllFirmwares()
        {
            _logger.LogInformation($"Checking [{_firmwarePath}]");
            if (!Directory.Exists(_firmwarePath))
            {
                _logger.LogInformation($"Path [{Path.GetFullPath(_firmwarePath)}] does not exist");
                return Enumerable.Empty<Firmware>();
            }

            return Directory.GetFiles(_firmwarePath)
                .Select(n =>
                {
                    _logger.LogInformation($"Found: [{n}]");
                    var s = TryGetFromFile(n, out var firmware);
                    return (s, firmware);
                })
                .Unpack();
        }

        public bool TryGetCurrentVersion(out Version version)
        {
            version = GetCurrentVersion();
            return version is not null;
        }

        public Version GetCurrentVersion() => GetAllFirmwares().Max(f => f.Version);

        public Stream GetCurrentFirmware()
        {
            var version = GetCurrentVersion();
            var filePath = Path.Combine(_firmwarePath, $"firmware.{version}.bin");
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public static bool TryGetFromFile(string filename, out Firmware firmware)
        {
            var file = new FileInfo(filename);
            if (!TryExtractVersionFromFileName(file.Name, out var version))
            {
                firmware = null;
                return false;
            }

            firmware = new Firmware
            {
                Version = version,
                Size = file.Length,
            };
            return true;
        }

        public static bool TryExtractVersionFromFileName(string filename, out Version version)
        {
            var match = FirmwareVersionExtract.Match(Path.GetFileName(filename));
            if (!match.Success)
            {
                version = null;
                return false;
            }

            return Version.TryParse(match.Groups["version"].Value, out version);
        }

        private static readonly Regex FirmwareVersionExtract = new(@"firmware\.(?<version>.+)\.bin", RegexOptions.Compiled);
    }
}
