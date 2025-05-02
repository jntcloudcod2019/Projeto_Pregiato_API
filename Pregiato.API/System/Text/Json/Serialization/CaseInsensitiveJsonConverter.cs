using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pregiato.API.System.Text.Json.Serialization
{
    public class CaseInsensitiveJsonConverter<T> : JsonConverter<T> where T : class
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Clona os options para não alterar os globais
            var customOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var converter in options.Converters)
            {
                customOptions.Converters.Add(converter);
            }

            return JsonSerializer.Deserialize<T>(ref reader, customOptions);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}