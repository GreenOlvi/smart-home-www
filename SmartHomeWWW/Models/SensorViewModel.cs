using SmartHomeCore.Domain;
using System;

namespace SmartHomeWWW.Models
{
    public class SensorViewModel
    {
        public Guid Id { get; init; }
        public string Mac { get; init; }
        public string Alias { get; init; }
        public string ChipType { get; init; }
        public DateTime? LastContact { get; init; }
        public string FirmwareVersion { get; init; }

        public static SensorViewModel FromSensor(Sensor sensor) =>
            new()
            {
                Id = sensor.Id,
                Mac = sensor.Mac,
                Alias = sensor.Alias,
                ChipType = sensor.ChipType,
                LastContact = sensor.LastContact,
                FirmwareVersion = sensor.FirmwareVersion,
            };
    }
}