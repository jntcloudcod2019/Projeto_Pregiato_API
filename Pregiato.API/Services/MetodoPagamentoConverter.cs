using Pregiato.API.Services.ServiceModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pregiato.API.Services
{
    public class MetodoPagamentoConverter : JsonConverter<MetodoPagamento>
    {
        public override MetodoPagamento Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            return MetodoPagamento.Create(value); 
        }

        public override void Write(Utf8JsonWriter writer, MetodoPagamento value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString()); 
        }
    }
}
