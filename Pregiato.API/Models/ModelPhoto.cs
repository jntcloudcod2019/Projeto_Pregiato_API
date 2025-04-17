namespace Pregiato.API.Models
{
    public class ModelPhoto
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public byte[]? ImageData { get; set; } = null!;
        public string? ImageName { get; set; }
        public string? ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
