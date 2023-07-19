using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Infrastructure;

public class JsonConverter<T> : ValueConverter<T?, string>
{
    public JsonConverter() : base(v => Serialize(v), v => Deserialize(v))
    {
    }

    private static string Serialize(T? value) => JsonSerializer.Serialize(value, SerializerOptions);
    private static T? Deserialize(string value) => JsonSerializer.Deserialize<T>(value, SerializerOptions);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
