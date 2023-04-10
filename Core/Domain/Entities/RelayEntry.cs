using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.Entities;

[Table("Relays")]
public record RelayEntry
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

    [NotMapped]
    public TasmotaClientKind? Kind => GetKind(this);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static TasmotaClientKind? GetKind(RelayEntry relay)
    {
        if (relay.Config is not JsonElement je)
        {
            return null;
        }

        if (!je.TryGetProperty(nameof(TasmotaMqttClientConfig.Kind), out var kindProperty)
            || !Enum.TryParse<TasmotaClientKind>(kindProperty.GetString(), out var k))
        {
            return null;
        }

        return k;
    }
}
