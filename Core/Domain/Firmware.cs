using System;

namespace SmartHomeWWW.Core.Domain
{
    public record Firmware
    {
        public Version Version { get; init; }
        public long Size { get; init; }
    }
}
