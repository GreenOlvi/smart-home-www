using System.Text.RegularExpressions;
using SmartHomeWWW.Core.Domain;
using SmartHomeWWW.Core.Utils;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Server.Config;

namespace SmartHomeWWW.Server.Firmwares
{
    public class DiskFirmwareRepository : IFirmwareRepository
    {
        public DiskFirmwareRepository(ILogger<DiskFirmwareRepository> logger, FirmwaresConfig config)
        {
            _logger = logger;
            _firmwarePath = config.Path;
        }

        private readonly ILogger<DiskFirmwareRepository> _logger;
        private readonly string _firmwarePath;

        public IEnumerable<Firmware> GetAllFirmwares()
        {
            _logger.LogDebug("Checking [{path}]", _firmwarePath);
            if (!Directory.Exists(_firmwarePath))
            {
                _logger.LogWarning("Path [{path}] does not exist", Path.GetFullPath(_firmwarePath));
                return Enumerable.Empty<Firmware>();
            }

            return Directory.GetFiles(_firmwarePath)
                .Select(n =>
                {
                    _logger.LogDebug("Found: [{name}]", n);
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

        public Version GetCurrentVersion() => GetAllFirmwares().Max(f => f.Version) ?? new Version();

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
                firmware = new Firmware();
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
            if (!match.Success || !Version.TryParse(match.Groups["version"].Value, out var parsedVersion))
            {
                version = new Version();
                return false;
            }

            version = parsedVersion;
            return true;
        }

        private readonly static Regex FirmwareVersionExtract = new(@"firmware\.(?<version>.+)\.bin", RegexOptions.Compiled);
    }
}
