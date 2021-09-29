using SmartHomeCore.Domain;
using System;

namespace SmartHomeWWW.Models
{
    public record FirmwareViewModel
    {
        public Version Version { get; init; }
        public long Size { get; init; }

        public static FirmwareViewModel FromFirmware(Firmware firmware) =>
            new() { Version = firmware.Version, Size = firmware.Size };
    }
}
