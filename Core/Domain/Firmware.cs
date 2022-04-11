using System;

namespace SmartHomeWWW.Core.Domain
{
    public record Firmware
    {
        public Version Version { get; init; } = new Version();
        public long Size { get; init; }
    }
}
