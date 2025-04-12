using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pregiato.API.System.Text.Json
{
    public class JsonByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = JsonSerializer.Deserialize<List<int>>(ref reader, options);
            return list?.Select(Convert.ToByte).ToArray() ?? Array.Empty<byte>();
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var b in value)
                writer.WriteNumberValue(b);
            writer.WriteEndArray();
        }
    }
}
