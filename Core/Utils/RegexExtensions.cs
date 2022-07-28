using System.Text.RegularExpressions;

namespace SmartHomeWWW.Core.Utils;

public static class RegexExtensions
{
    public static bool TryMatch(this Regex regex, string text, out Match match)
    {
        match = regex.Match(text);
        return match.Success;
    }

    public static bool TryMatch(this Regex regex, string text, string groupName, out string value)
    {
        var match = regex.Match(text);

        if (match.Success)
        {
            value = match.Groups[groupName].Value;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
