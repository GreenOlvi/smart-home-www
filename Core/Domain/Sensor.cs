using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHomeWWW.Core.Domain
{
    public class Sensor
    {
        [Key]
        public Guid Id { get; init; }
        public string Mac { get; init; }
        public string Alias { get; set; }
        public string ChipType { get; set; }
        public DateTime? LastContact { get; set; }
        public string FirmwareVersion { get; set; }

        public override string ToString() => $"{ChipType} {Alias ?? Mac}";
    }
}
