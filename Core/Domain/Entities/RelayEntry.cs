using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.Entities;

[Table("Relays")]
public class RelayEntry
{
    [Key]
    public Guid Id { get; init; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [Column("Config")]
    public string ConfigSerialized
    {
        get => JsonSerializer.Serialize(Config, SerializerOptions);
        set => Config = JsonSerializer.Deserialize<object>(value, SerializerOptions) ?? new { };
    }

    [NotMapped]
    public object Config { get; set; } = new { };

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
