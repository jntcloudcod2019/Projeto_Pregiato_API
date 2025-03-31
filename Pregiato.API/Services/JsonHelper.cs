using System.Text.Json;

namespace Pregiato.API.Services
{
    public static class JsonHelper
    {
        public static JsonDocument SerializeToDocument<T>(T value)
        {
            string json = JsonSerializer.Serialize(value);
            return JsonDocument.Parse(json);
        }
    }
}
