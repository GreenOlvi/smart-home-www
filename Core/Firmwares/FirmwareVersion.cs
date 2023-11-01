using System.Text.RegularExpressions;

namespace SmartHomeWWW.Core.Firmwares;

public readonly partial record struct FirmwareVersion
{
    public Version Prefix { get; init; } = new Version();
    public string? Suffix { get; init; } = null;

    private static readonly Regex CombinedVersionPattern = BuildCombinedVersionPattern();

    public FirmwareVersion()
    {
    }

    public override string ToString() => Suffix is null ? Prefix.ToString() : $"{Prefix}-{Suffix.ToLowerInvariant()}";

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

    [GeneratedRegex(@"^(?<prefix>\d+\.\d+\.\d+)(-(?<suffix>.+))?$", RegexOptions.Compiled)]
    private static partial Regex BuildCombinedVersionPattern();
}
