using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SmartHomeWWW.Logic
{
    public static class FirmwareUtils
    {
        private static readonly Regex FirmwareVersionExtract = new(@"firmware\.(?<version>.+)\.bin", RegexOptions.Compiled);

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
    }
}
