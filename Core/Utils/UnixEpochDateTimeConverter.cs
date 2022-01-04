using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Utils
{
    public class UnixEpochDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()).UtcDateTime;

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((ulong)Math.Max((value - DateTime.UnixEpoch).TotalSeconds, 0));
        }
    }
}
