using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHomeWWW.Core.Domain.Entities;

public class Sensor
{
    [Key]
    public Guid Id { get; init; }
    public string Mac { get; init; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string ChipType { get; set; } = string.Empty;
    public DateTime? LastContact { get; set; }
    public string FirmwareVersion { get; set; } = string.Empty;
    public string? UpdateChannel { get; set; }

    public override string ToString() => $"{ChipType} {Alias ?? Mac}";
}
