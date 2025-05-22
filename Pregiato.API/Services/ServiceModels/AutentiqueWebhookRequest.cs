using System.Text.Json.Serialization;

namespace Pregiato.API.Services.ServiceModels
{
    public class AutentiqueWebhookRequest
    {
        [JsonIgnore]
        public string? Event { get; set; }
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
