namespace Pregiato.API.Response
{
    public class PhotoModelResponse
    {
        public Guid Id { get; set; }
        public string? ImageName { get; set; }
        public string? ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? ImageBase64 { get; set; }
        public string? Message { get; set; }
    }
}
