using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHomeCore.Domain
{
    public class Sensor
    {
        [Key]
        public Guid Id { get; init; }
        [MaxLength(17)]
        public string Mac { get; init; }
        public string Alias { get; set; }
        public string ChipType { get; set; }
        public DateTime? LastContact { get; set; }
        public string FirmwareVersion { get; set; }
    }
}
