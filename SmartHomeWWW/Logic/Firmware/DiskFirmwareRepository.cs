using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartHomeWWW.Logic.Firmware
{
    public class DiskFirmwareRepository : IFirmwareRepository
    {
        public DiskFirmwareRepository(string firmwarePath)
        {
            _firmwarePath = firmwarePath;
        }

        private readonly string _firmwarePath;

        public IEnumerable<Version> GetAllVersions()
        {
            if (!Directory.Exists(_firmwarePath))
            {
                return Enumerable.Empty<Version>();
            }

            return Directory.GetFiles(_firmwarePath, "firmware.*.bin")
                .Select(n => Path.GetFileName(n))
                .Select(n =>
                {
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
