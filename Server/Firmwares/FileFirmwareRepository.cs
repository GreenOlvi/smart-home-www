﻿using Microsoft.Extensions.Options;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Utils;
using SmartHomeWWW.Server.Config;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace SmartHomeWWW.Server.Firmwares;

public partial class FileFirmwareRepository(ILogger<FileFirmwareRepository> logger, IOptionsSnapshot<FirmwaresConfig> config, IFileSystem fileSystem) : IFirmwareRepository
{
    private readonly ILogger<FileFirmwareRepository> _logger = logger;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly string _firmwarePath = config.Value.Path;

    public IEnumerable<IFirmware> GetAllFirmwares()
    {
        _logger.LogDebug("Checking [{Path}]", _firmwarePath);
        if (!_fileSystem.Directory.Exists(_firmwarePath))
        {
            _logger.LogWarning("Path [{Path}] does not exist", _fileSystem.Path.GetFullPath(_firmwarePath));
            return Enumerable.Empty<IFirmware>();
        }

        return _fileSystem.Directory.GetFiles(_firmwarePath)
            .Select(n =>
            {
                _logger.LogDebug("Found: [{Name}]", n);
                var s = TryGetFromFile(n, out var firmware);
                return (s, firmware);
            })
            .Unpack();
    }

    private bool TryGetFromFile(string filename, out IFirmware firmware)
    {
        var file = _fileSystem.FileInfo.New(filename);
        if (!TryExtractVersionFromFileName(file.Name, out var version))
        {
            firmware = FileFirmware.NullFirmware;
            return false;
        }

        var channel = GetChannelFromSuffix(version.Suffix);
        firmware = new FileFirmware(filename, version, file.Length, channel, () => _fileSystem.File.OpenRead(filename));
        return true;
    }

    private static UpdateChannel GetChannelFromSuffix(string? suffix)
    {
        if (suffix is null)
        {
            return UpdateChannel.Stable;
        }

        return Enum.TryParse<UpdateChannel>(suffix, true, out var ch)
            ? ch
            : UpdateChannel.Unknown;
    }

    public static bool TryExtractVersionFromFileName(string filename, out FirmwareVersion version)
    {
        var match = FirmwareVersionExtract.Match(Path.GetFileName(filename));
        if (match.Success && Version.TryParse(match.Groups["version"].Value, out var parsedVersion))
        {
            version = new FirmwareVersion
            {
                Prefix = parsedVersion,
                Suffix = match.Groups["suffix"].Success ? match.Groups["suffix"].Value : null,
            };
            return true;
        }

        version = new FirmwareVersion();
        return false;
    }

    private static readonly Regex FirmwareVersionExtract = BuildFirmwareVersionPattern();

    [GeneratedRegex(@"firmware\.(?<version>[^-]+)(-(?<suffix>.+))?\.bin", RegexOptions.Compiled)]
    private static partial Regex BuildFirmwareVersionPattern();
}
