using System.Text.RegularExpressions;

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

    public static bool TryParse(string text, out FirmwareVersion result)
    {
        var match = CombinedVersionPattern.Match(text);
        if (!match.Success)
        {
            result = new FirmwareVersion();
            return false;
        }

        var prefix = new Version(match.Groups["prefix"].Value);
        var suffix = match.Groups["suffix"].Success ? match.Groups["suffix"].Value : null;
        result = new FirmwareVersion { Prefix = prefix, Suffix = suffix };

        return true;
    }

    public static bool operator ==(FirmwareVersion version, string other) => version.ToString() == other;
    public static bool operator !=(FirmwareVersion version, string other) => version.ToString() != other;
}
