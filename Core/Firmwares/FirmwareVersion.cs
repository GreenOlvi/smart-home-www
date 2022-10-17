﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartHomeWWW.Core.Firmwares;

public record FirmwareVersion
{
    public Version Prefix { get; init; } = new Version();
    public string? Suffix { get; init; }

    public override string ToString() => Suffix is null ? Prefix.ToString() : $"{Prefix}-{Suffix.ToLowerInvariant()}";

    private static readonly Regex CombinedVersionPattern = new(@"^(?<prefix>\d+\.\d+\.\d+)(-(?<suffix>.+))?$", RegexOptions.Compiled);
    public static FirmwareVersion Parse(string text)
    {
        var match = CombinedVersionPattern.Match(text);
        if (!match.Success)
        {
            throw new InvalidOperationException("Invalid version format");
        }

        var prefix = new Version(match.Groups["prefix"].Value);
        var suffix = match.Groups["suffix"].Success ? match.Groups["suffix"].Value : null;
        return new FirmwareVersion { Prefix = prefix, Suffix = suffix };
    }

    public static bool operator ==(FirmwareVersion version, string other) => version.ToString() == other;
    public static bool operator !=(FirmwareVersion version, string other) => version.ToString() != other;
}
