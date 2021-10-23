using System;

namespace SmartHomeCore.Domain
{
    public class Sensor
    {
        public Guid Id { get; init; }
        public string Mac { get; init; }
        public string Alias { get; init; }
        public string ChipType { get; init; }
        public DateTime? LastContact { get; init; }
        public string FirmwareVersion { get; init; }
    }
}
