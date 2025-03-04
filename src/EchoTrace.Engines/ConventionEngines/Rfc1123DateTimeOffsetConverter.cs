using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EchoTrace.Engines.ConventionEngines;

public class Rfc1123DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var utcValue = value.ToUniversalTime();
        writer.WriteStringValue(utcValue.ToString("R", CultureInfo.InvariantCulture)); // 使用 RFC1123 格式
    }
}