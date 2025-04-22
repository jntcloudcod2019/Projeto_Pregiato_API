namespace Pregiato.API.Response
{
    public class TrainingResponse
    {
        public Guid ID { get; set; }
        public string NAME { get; set; } = string.Empty;
        public string INSTRUCTOR { get; set; } = string.Empty;
        public string? DESCRIPTION { get; set; }
        public string? MESSAGE { get; set; }
    }
}
