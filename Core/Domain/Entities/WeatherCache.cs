using System.ComponentModel.DataAnnotations;

namespace SmartHomeWWW.Core.Domain.Entities;

public record WeatherCache
{
    [Key]
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public DateTime? Expires { get; init; }
    public string Data { get; set; } = string.Empty;
}
