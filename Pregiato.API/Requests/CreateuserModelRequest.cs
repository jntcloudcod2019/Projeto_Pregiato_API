using System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class CreateuserModelRequest
    {
        [JsonPropertyName("FullName")]
        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? NomeProducers { get; set; }
    }
}
